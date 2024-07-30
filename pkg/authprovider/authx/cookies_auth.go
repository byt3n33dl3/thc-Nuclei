package authx

import (
	"net/http"

	"github.com/projectdiscovery/retryablehttp-go"
)

var (
	_ AuthStrategy = &CookiesAuthStrategy{}
)

// CookiesAuthStrategy is a strategy for cookies auth
type CookiesAuthStrategy struct {
	Data *Secret
}

// NewCookiesAuthStrategy creates a new cookies auth strategy
func NewCookiesAuthStrategy(data *Secret) *CookiesAuthStrategy {
	return &CookiesAuthStrategy{Data: data}
}

// Apply applies the cookies auth strategy to the request
func (s *CookiesAuthStrategy) Apply(req *http.Request) {
	for _, cookie := range s.Data.Cookies {
		exists := false

		// check for existing cookies
		for _, c := range req.Cookies() {
			if c.Name == cookie.Key {
				exists = true
				break
			}
		}

		if !exists {
			req.AddCookie(&http.Cookie{
				Name:  cookie.Key,
				Value: cookie.Value,
			})
		}
	}
}

// ApplyOnRR applies the cookies auth strategy to the retryable request
func (s *CookiesAuthStrategy) ApplyOnRR(req *retryablehttp.Request) {
	for _, cookie := range s.Data.Cookies {
		exists := false

		// check for existing cookies
		for _, c := range req.Cookies() {
			if c.Name == cookie.Key {
				exists = true
				break
			}
		}

		if !exists {
			req.AddCookie(&http.Cookie{
				Name:  cookie.Key,
				Value: cookie.Value,
			})
		}
	}
}
