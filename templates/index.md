# {{displayName}}

{{#if overviewImage}}
![{{name}}]({{overviewImage}})

{{/if}}
{{#if description}}
{{description}}

{{/if}}
* Name: `{{name}}`
* Display name: {{displayName}}
* Version: {{version}}
* [Git URL]({{repositoryUrl}})

The package's components are described on the [Components](manual/components.md) page, and its samples on the [Samples](manual/samples.md) page.

## Dependencies

{{#if unity}}
This version of the package was developed and tested with Unity {{unity}}{{#if dependencies}}, with the following package dependencies{{/if}}.

{{/if}}
{{#unless unity}}
{{#if dependencies}}
The package has the following dependencies.

{{/if}}
{{/unless}}
{{#if dependencies}}
{{#each dependencies}}
* {{#if gitUrl}}[{{name}}]({{documentationUrl}}) ([Git URL]({{gitUrl}})){{/if}}{{#unless gitUrl}}{{#if documentationUrl}}[{{name}}@{{version}}]({{documentationUrl}}){{/if}}{{#unless documentationUrl}}{{name}}@{{version}}{{/unless}}{{/unless}}
{{/each}}

{{/if}}
{{#if xrcDependencies}}
The Package Manager does not install Git dependencies automatically. Add each package that has a Git URL to the project before installing this package.

{{/if}}
## Installation

{{#if xrcDependencies}}
Add the Git dependencies listed under [Dependencies](#dependencies) first. Then, in the Package Manager, choose **Add package from git URL** and enter the package's [Git URL]({{repositoryUrl}}).
{{/if}}
{{#unless xrcDependencies}}
In the Package Manager, choose **Add package from git URL** and enter the package's [Git URL]({{repositoryUrl}}).
{{/unless}}

See Unity's manual: [Install a UPM package from a Git URL](https://docs.unity3d.com/6000.0/Documentation/Manual/upm-ui-giturl.html). The samples are under the Samples tab of the package.

{{#if private}}
> [!NOTE]
> This documentation is public, but the package is not. Its repository is private to {{author}}. Installing the package requires a GitHub account that {{author}} has given access to the repository; without it, the Package Manager cannot download the package.
{{/if}}
