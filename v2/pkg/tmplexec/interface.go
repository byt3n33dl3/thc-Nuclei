package tmplexec

import (
	"github.com/projectdiscovery/nuclei/v2/pkg/protocols"
	"github.com/projectdiscovery/nuclei/v2/pkg/protocols/common/contextargs"
	"github.com/projectdiscovery/nuclei/v2/pkg/tmplexec/flow"
	"github.com/projectdiscovery/nuclei/v2/pkg/tmplexec/generic"
	"github.com/projectdiscovery/nuclei/v2/pkg/tmplexec/multiproto"
)

var (
	_ TemplateEngine = &generic.Generic{}
	_ TemplateEngine = &flow.FlowExecutor{}
	_ TemplateEngine = &multiproto.MultiProtocol{}
)

// TemplateEngine is a template executor with different functionality
// Ex:
// 1. generic => executes all protocol requests one after another (Done)
// 2. flow  => executes protocol requests based on how they are defined in flow (Done)
// 3. multiprotocol => executes multiple protocols in parallel (Done)
type TemplateEngine interface {
	// Note: below methods only need to implement extra / engine specific functionality
	// basic request compilation , callbacks , cli output callback etc are handled by `TemplateExecuter` and no need to do it again
	// Extra Compilation (if any)
	Compile() error

	// ExecuteWithResults executes the template and returns results
	ExecuteWithResults(input *contextargs.Context, callback protocols.OutputEventCallback) error

	// Name returns name of template engine
	Name() string
}