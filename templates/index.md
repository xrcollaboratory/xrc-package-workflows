# {{displayName}}

{{#if overviewImage}}
![{{name}}]({{overviewImage}})

{{/if}}
{{#if description}}
{{description}}

{{/if}}
{{#if components}}
## Components

{{#each components}}
* [{{label}}](manual/components.md#{{anchor}}){{#if summary}}: {{summary}}{{/if}}
{{/each}}

{{/if}}
{{#if samples}}
## Samples

{{#each samples}}
* [{{displayName}}](manual/samples.md#{{anchor}}){{#if description}}: {{description}}{{/if}}
{{/each}}

{{/if}}
## Installation

{{#if unity}}
This version of the package was developed and tested with Unity {{unity}}.

{{/if}}
{{#if xrcDependencies}}
Add these XRC Toolkit packages first. The Package Manager does not install them for you:

{{#each xrcDependencies}}
* [{{name}}]({{documentationUrl}}) {{version}} or later: `{{gitUrl}}`
{{/each}}

{{/if}}
{{#if unityDependencies}}
These Unity packages are installed automatically:

{{#each unityDependencies}}
* {{#if documentationUrl}}[{{name}}]({{documentationUrl}}){{/if}}{{#unless documentationUrl}}{{name}}{{/unless}} {{version}}
{{/each}}

{{/if}}
In the Package Manager, choose **Add package from git URL** and enter:

```
{{repositoryUrl}}
```

See Unity's manual: [Install a UPM package from a Git URL](https://docs.unity3d.com/6000.0/Documentation/Manual/upm-ui-giturl.html). The samples are under the Samples tab of the package.

{{#if private}}
> [!NOTE]
> This documentation is public, but the package is not. Its repository is private to {{author}}. Installing the package requires a GitHub account that {{author}} has given access to the repository; without it, the Package Manager cannot download the package.

{{/if}}
## Package

* Name: `{{name}}`
* Display name: {{displayName}}
* Version: {{version}} ([changelog](changelog/CHANGELOG.md))
* [Repository]({{repositoryUrl}})
* [License](license/LICENSE.md)
