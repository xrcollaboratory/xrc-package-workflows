# {{displayName}}

{{#if overviewImage}}
![{{name}}]({{overviewImage}})

{{/if}}
{{#if description}}
{{description}}

{{/if}}
* Name: `{{name}}`
* Display name: {{displayName}}
* Version: {{version}} ([changelog](changelog/CHANGELOG.md))
* [Git URL]({{repositoryUrl}})
* [License](license/LICENSE.md)

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
{{#if xrcDependencies}}
### XRC Toolkit packages

The Package Manager does not install these automatically, and the package cannot be installed while they are missing from the project. Add each one from its Git URL first.

{{#each xrcDependencies}}
* [{{name}}@{{version}}]({{documentationUrl}}) ([Git URL]({{gitUrl}}))
{{/each}}

{{/if}}
{{#if unityDependencies}}
### Unity packages

The Package Manager installs these automatically.

{{#each unityDependencies}}
* {{#if documentationUrl}}[{{name}}@{{version}}]({{documentationUrl}}){{/if}}{{#unless documentationUrl}}{{name}}@{{version}}{{/unless}}
{{/each}}

{{/if}}
## Installation

{{#if xrcDependencies}}
Add the XRC Toolkit packages listed under [Dependencies](#dependencies) first. Then, in the Package Manager, choose **Add package from git URL** and enter the package's [Git URL]({{repositoryUrl}}).
{{/if}}
{{#unless xrcDependencies}}
In the Package Manager, choose **Add package from git URL** and enter the package's [Git URL]({{repositoryUrl}}).
{{/unless}}

See Unity's manual: [Install a UPM package from a Git URL](https://docs.unity3d.com/6000.0/Documentation/Manual/upm-ui-giturl.html). The samples are under the Samples tab of the package.

{{#if private}}
> [!NOTE]
> This documentation is public, but the package is not. Its repository is private to {{author}}. Installing the package requires a GitHub account that {{author}} has given access to the repository; without it, the Package Manager cannot download the package.
{{/if}}
