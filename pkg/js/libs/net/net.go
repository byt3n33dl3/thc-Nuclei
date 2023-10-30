package net

import (
	"bytes"
	"context"
	"crypto/tls"
	"encoding/hex"
	"errors"
	"fmt"
	"net"
	"syscall"
	"time"

	"github.com/projectdiscovery/gologger"
	"github.com/projectdiscovery/nuclei/v3/pkg/protocols/common/protocolstate"
	"github.com/projectdiscovery/nuclei/v3/pkg/types"
)

// Open opens a new connection to the address with a timeout.
// supported protocols: tcp, udp
func Open(protocol, address string) (*NetConn, error) {
	conn, err := protocolstate.Dialer.Dial(context.TODO(), protocol, address)
	if err != nil {
		return nil, err
	}
	return &NetConn{conn: conn}, nil
}

// Open opens a new connection to the address with a timeout.
// supported protocols: tcp, udp
func OpenTLS(protocol, address string) (*NetConn, error) {
	config := &tls.Config{InsecureSkipVerify: true, MinVersion: tls.VersionTLS10}
	host, _, _ := net.SplitHostPort(address)
	if host != "" {
		c := config.Clone()
		c.ServerName = host
		config = c
	}
	conn, err := protocolstate.Dialer.DialTLSWithConfig(context.TODO(), protocol, address, config)
	if err != nil {
		return nil, err
	}
	return &NetConn{conn: conn}, nil
}

// NetConn is a connection to a remote host.
type NetConn struct {
	conn    net.Conn
	timeout time.Duration
}

// Close closes the connection.
func (c *NetConn) Close() error {
	err := c.conn.Close()
	return err
}

// SetTimeout sets read/write timeout for the connection (in seconds).
func (c *NetConn) SetTimeout(value int) {
	c.timeout = time.Duration(value) * time.Second
}

// setDeadLine sets read/write deadline for the connection (in seconds).
// this is intended to be called before every read/write operation.
func (c *NetConn) setDeadLine() {
	if c.timeout == 0 {
		c.timeout = 5 * time.Second
	}
	_ = c.conn.SetDeadline(time.Now().Add(c.timeout))
}

// SendArray sends array data to connection
func (c *NetConn) SendArray(data []interface{}) error {
	c.setDeadLine()
	input := types.ToByteSlice(data)
	length, err := c.conn.Write(input)
	if err != nil {
		return err
	}
	if length < len(input) {
		return fmt.Errorf("failed to write all bytes (%d bytes written, %d bytes expected)", length, len(input))
	}
	return nil
}

// SendHex sends hex data to connection
func (c *NetConn) SendHex(data string) error {
	c.setDeadLine()
	bin, err := hex.DecodeString(data)
	if err != nil {
		return err
	}
	length, err := c.conn.Write(bin)
	if err != nil {
		return err
	}
	if length < len(bin) {
		return fmt.Errorf("failed to write all bytes (%d bytes written, %d bytes expected)", length, len(bin))
	}
	return nil
}

// Send sends data to the connection with a timeout.
func (c *NetConn) Send(data string) error {
	c.setDeadLine()
	bin := []byte(data)
	length, err := c.conn.Write(bin)
	if err != nil {
		return err
	}
	if length < len(bin) {
		return fmt.Errorf("failed to write all bytes (%d bytes written, %d bytes expected)", length, len(data))
	}
	return nil
}

// Recv receives data from the connection with a timeout.
// If N is 0, it will read all data sent by the server.
func (c *NetConn) Recv(N int) ([]byte, error) {
	c.setDeadLine()
	var buff bytes.Buffer
	defer func() {
		if r := recover(); r != nil {
			gologger.Error().Msgf("unbounded read from connection caused a panic: %v", r)
		}
	}()
	if buff.Len() < N || N == 0 {
		_, err := buff.ReadFrom(c.conn)
		if err != nil {
			var netErr net.Error
			if !(errors.As(err, &netErr) && netErr.Timeout()) && !errors.Is(err, syscall.ECONNREFUSED) { // timeout error or connection refused
				return []byte{}, err
			}
		}
	}
	// don't return more than N bytes even if we read more
	if N > 0 && buff.Len() > N {
		return buff.Bytes()[:N], nil
	}
	return buff.Bytes(), nil
}

// RecvString receives data from the connection with a timeout
// output is returned as a string.
// If N is 0, it will read all data sent by the server.
func (c *NetConn) RecvString(N int) (string, error) {
	bin, err := c.Recv(N)
	if err != nil {
		return "", err
	}
	return string(bin), nil
}

// RecvHex receives data from the connection with a timeout
// in hex format.
// If N is 0,it will read all data sent by the server.
func (c *NetConn) RecvHex(N int) (string, error) {
	bin, err := c.Recv(N)
	if err != nil {
		return "", err
	}
	return hex.Dump(bin), nil
}
