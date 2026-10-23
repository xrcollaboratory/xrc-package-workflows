# {{displayName}}

{{#if overviewImage}}
![{{name}}]({{overviewImage}})

{{/if}}
{{#if description}}
{{description}}

{{/if}}
## Technical details

### Package

* Name: {{name}}
* Display name: {{displayName}}
* Version: {{version}}
{{#if repositoryUrl}}
* [Git URL]({{repositoryUrl}})
{{/if}}

### Requirements

{{#if unity}}
This version of the {{displayName}} package is compatible with the following versions of the Unity Editor:

* {{unity}} and later

{{/if}}
{{#if dependencies}}
### Dependencies

The {{displayName}} package has the following dependencies which are automatically added to your project when installing:

{{#each dependencies}}
* {{name}}@{{version}}
{{/each}}

If the package has additional Git dependencies listed in the package description, such as XRC Toolkit packages, then they need to be installed manually via the Unity Package Manager.

{{/if}}
### Installation

To install the package, follow these steps:
1. In the Unity Editor, click on **Window > Package Manager**
2. Click the + button and choose **Add package from git URL** option
3. Enter the package's Git URL. Make sure the URL has ".git" ending.
4. If the repository is private, you will be asked to authenticate via your GitHub account. If you haven't been granted access to the repository you will not be able to install the package.
5. The package should be installed into your project.
6. You can download the package samples from under the Samples tab in the Package Manager

From Unity: [Install a UPM package from a Git URL](https://docs.unity3d.com/6000.0/Documentation/Manual/upm-ui-giturl.html)

> [!NOTE]
> Even though the package documentation is public, the package itself and its repository might be private and not accessible. If you haven't been granted access to the repository you will not be able to install the package.
