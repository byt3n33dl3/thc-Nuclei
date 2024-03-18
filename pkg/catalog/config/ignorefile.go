package config

import (
	"os"

	"github.com/projectdiscovery/gologger"
	"gopkg.in/yaml.v2"
)

// IgnoreFile is an internal nuclei template blocking configuration file
type IgnoreFile struct {
	Tags  []string `yaml:"tags"`
	Files []string `yaml:"files"`
}

// ReadIgnoreFile reads the nuclei ignore file returning blocked tags and paths
func ReadIgnoreFile(filters ...func(*IgnoreFile)) IgnoreFile {
	file, err := os.Open(DefaultConfig.GetIgnoreFilePath())
	if err != nil {
		gologger.Error().Msgf("Could not read nuclei-ignore file: %s\n", err)
		return IgnoreFile{}
	}
	defer file.Close()

	ignore := IgnoreFile{}
	if err := yaml.NewDecoder(file).Decode(&ignore); err != nil {
		gologger.Error().Msgf("Could not parse nuclei-ignore file: %s\n", err)
		return IgnoreFile{}
	}

	for _, filter := range filters {
		if filter != nil {
			filter(&ignore)
		}
	}
	return ignore
}
