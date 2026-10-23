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

## Installation

{{#if unity}}
This version of the package was developed and tested with Unity {{unity}}.

{{/if}}
{{#if xrcDependencies}}
The package depends on the following XRC Toolkit packages. The Package Manager does not install them automatically, and the package cannot be installed while they are missing from the project, so add each one from its Git URL first:

{{#each xrcDependencies}}
* [{{name}}]({{documentationUrl}}) {{version}} or later ([Git URL]({{gitUrl}}))
{{/each}}

{{/if}}
{{#if unityDependencies}}
The package also depends on the following Unity packages, which the Package Manager installs automatically:

{{#each unityDependencies}}
* {{#if documentationUrl}}[{{name}}]({{documentationUrl}}){{/if}}{{#unless documentationUrl}}{{name}}{{/unless}} {{version}}
{{/each}}

{{/if}}
In the Package Manager, choose **Add package from git URL** and enter the package's [Git URL]({{repositoryUrl}}).

See Unity's manual: [Install a UPM package from a Git URL](https://docs.unity3d.com/6000.0/Documentation/Manual/upm-ui-giturl.html). The samples are under the Samples tab of the package.

{{#if private}}
> [!NOTE]
> This documentation is public, but the package is not. Its repository is private to {{author}}. Installing the package requires a GitHub account that {{author}} has given access to the repository; without it, the Package Manager cannot download the package.

{{/if}}