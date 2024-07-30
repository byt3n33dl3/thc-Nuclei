package authx

import (
	"net/http"

	"github.com/projectdiscovery/retryablehttp-go"
)

var (
	_ AuthStrategy = &BasicAuthStrategy{}
)

// BasicAuthStrategy is a strategy for basic auth
type BasicAuthStrategy struct {
	Data *Secret
}

// NewBasicAuthStrategy creates a new basic auth strategy
func NewBasicAuthStrategy(data *Secret) *BasicAuthStrategy {
	return &BasicAuthStrategy{Data: data}
}

// Apply applies the basic auth strategy to the request
func (s *BasicAuthStrategy) Apply(req *http.Request) {
	if _, _, exists := req.BasicAuth(); !exists {
		// NOTE(dwisiswant0): if the Basic auth is invalid, e.g. "Basic xyz",
		// `exists` will be `false`. I'm not sure if we should check it through
		// the presence of an "Authorization" header.
		req.SetBasicAuth(s.Data.Username, s.Data.Password)
	}
}

// ApplyOnRR applies the basic auth strategy to the retryable request
func (s *BasicAuthStrategy) ApplyOnRR(req *retryablehttp.Request) {
	if _, _, exists := req.BasicAuth(); !exists {
		// NOTE(dwisiswant0): See line 26-28.
		req.SetBasicAuth(s.Data.Username, s.Data.Password)
	}
}
