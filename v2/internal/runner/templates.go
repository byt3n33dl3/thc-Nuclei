package runner

import (
	"bytes"
	"os"
	"path/filepath"
	"sort"
	"strings"

	"github.com/alecthomas/chroma/quick"
	"github.com/logrusorgru/aurora"
	"github.com/projectdiscovery/nuclei/v2/pkg/catalog/loader"
	"github.com/projectdiscovery/nuclei/v2/pkg/utils"

	"github.com/projectdiscovery/gologger"
	"github.com/projectdiscovery/nuclei/v2/pkg/parsers"
	"github.com/projectdiscovery/nuclei/v2/pkg/templates"
	"github.com/projectdiscovery/nuclei/v2/pkg/types"
)

// log available templates for verbose (-vv)
func (r *Runner) logAvailableTemplate(tplPath string) {
	t, err := parsers.ParseTemplate(tplPath, r.catalog)
	if err != nil {
		gologger.Error().Msgf("Could not parse file '%s': %s\n", tplPath, err)
	} else {
		r.verboseTemplate(t)
	}
}

// log available templates for verbose (-vv)
func (r *Runner) verboseTemplate(tpl *templates.Template) {
	gologger.Print().Msgf("%s\n", templates.TemplateLogMessage(tpl.ID,
		types.ToString(tpl.Info.Name),
		tpl.Info.Authors.ToSlice(),
		tpl.Info.SeverityHolder.Severity))
}

// templateTags returns a slice of all available template tags
func (r *Runner) templateTags(store *loader.Store) []string {
	var tags []string
	for _, tpl := range store.Templates() {
		tags = append(tags, tpl.Info.Tags.ToSlice()...)
	}
	tags = utils.RemoveDuplicate(tags)
	sort.Strings(tags)

	return tags
}

// groupedTemplateTags returns a map of tags grouped by alphabet
func (r *Runner) groupedTemplateTags(store *loader.Store) map[string][]string {
	tagMap := make(map[string][]string)
	for _, tag := range r.templateTags(store) {
		if tag == "" {
			continue
		}
		groupingChar := tag[0:1]
		if groupingChar == "" {
			continue
		}
		tagMap[groupingChar] = append(tagMap[groupingChar], tag)
	}

	return tagMap
}

// listAvailableStoreTemplateTags list all available template tags
func (r *Runner) listAvailableStoreTemplateTags(store *loader.Store) {
	gologger.Print().Msgf(
		"\nListing available v.%s nuclei template tags for %s",
		r.templatesConfig.TemplateVersion,
		r.templatesConfig.TemplatesDirectory,
	)
	tagMap := r.groupedTemplateTags(store)

	groupingChars := make([]string, len(tagMap))
	i := 0
	for key := range tagMap {
		groupingChars[i] = key
		i++
	}
	sort.Strings(groupingChars)

	for _, groupingChar := range groupingChars {
		gologger.Silent().Msgf("\n%s\n", strings.ToUpper(groupingChar))
		gologger.Silent().Msgf("- %s", strings.Join(tagMap[groupingChar][:], ", "))
		// for _, tag := range tagMap[groupingChar] {
		// 	gologger.Silent().Msgf("- %s", tag)
		// }
	}
}

// listAvailableStoreTemplates list all avaiable templates
func (r *Runner) listAvailableStoreTemplates(store *loader.Store) {
	gologger.Print().Msgf(
		"\nListing available v.%s nuclei templates for %s",
		r.templatesConfig.TemplateVersion,
		r.templatesConfig.TemplatesDirectory,
	)
	for _, tpl := range store.Templates() {
		if hasExtraFlags(r.options) {
			if r.options.TemplateDisplay {
				colorize := !r.options.NoColor

				path := tpl.Path
				tplBody, err := os.ReadFile(path)
				if err != nil {
					gologger.Error().Msgf("Could not read the template %s: %s", path, err)
					continue
				}

				if colorize {
					path = aurora.Cyan(tpl.Path).String()
					tplBody, err = r.highlightTemplate(&tplBody)
					if err != nil {
						gologger.Error().Msgf("Could not hihglight the template %s: %s", tpl.Path, err)
						continue
					}

				}
				gologger.Silent().Msgf("Template: %s\n\n%s", path, tplBody)
			} else {
				gologger.Silent().Msgf("%s\n", strings.TrimPrefix(tpl.Path, r.templatesConfig.TemplatesDirectory+string(filepath.Separator)))
			}
		} else {
			r.verboseTemplate(tpl)
		}
	}
}

func (r *Runner) highlightTemplate(body *[]byte) ([]byte, error) {
	var buf bytes.Buffer
	// YAML lexer, true color terminar formatter and monokai style
	err := quick.Highlight(&buf, string(*body), "yaml", "terminal16m", "monokai")
	if err != nil {
		return nil, err
	}

	return buf.Bytes(), nil
}

func hasExtraFlags(options *types.Options) bool {
	return options.Templates != nil || options.Authors != nil ||
		options.Tags != nil || len(options.ExcludeTags) > 3 ||
		options.IncludeTags != nil || options.IncludeIds != nil ||
		options.ExcludeIds != nil || options.IncludeTemplates != nil ||
		options.ExcludedTemplates != nil || options.ExcludeMatchers != nil ||
		options.Severities != nil || options.ExcludeSeverities != nil ||
		options.Protocols != nil || options.ExcludeProtocols != nil ||
		options.IncludeConditions != nil || options.TemplateList
}
