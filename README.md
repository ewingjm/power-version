# PowerVersion![](./docs/images/logo.svg)

Version your Power Apps solutions at build-time automatically with Git.

## Table of contents

- [PowerVersion](#powerversion)
  - [Table of contents](#table-of-contents)
  - [Introduction](#introduction)
  - [Prerequisites](#prerequisites)
  - [Installation](#installation)
  - [Usage](#usage)
    - [Version increments](#version-increments)
      - [Metadata commits](#metadata-commits)
      - [Non-metadata commits](#non-metadata-commits)
      - [Version tags](#version-tags)
    - [Branch types](#branch-types)
      - [Configuration](#configuration)
      - [Mainline branches](#mainline-branches)
      - [Release branches](#release-branches)
      - [Topic branches](#topic-branches)
  - [Contributing](#contributing)


## Introduction

Generating [Semantic Versions](https://semver.org/)  from Git history is often a sensible approach to versioning artifacts. It is also made incredibly easy thanks to tools such as [GitVersion](https://github.com/GitTools/GitVersion) and [semantic-release](https://github.com/semantic-release/semantic-release). 

Power Apps solutions have some unique challenges when applying this approach:

- Multiple solutions often need to be versioned independently within the same repository
- Solution versions do not conform fully to the Semantic Versions specification

**PowerVersion** is designed to address these challenges and provide similar functionality to the tools mentioned above.

## Prerequisites


- You must have a solution project created with the Power App CLI
- Your solution project must exist within a Git repository
- Your Git workflow must be similar to Microsoft's [Release flow](https://learn.microsoft.com/en-us/devops/develop/how-microsoft-develops-devops#microsoft-release-flow)

## Installation

Install the [PowerVersion.MSBuild](https://www.nuget.org/packages/PowerVersion.MSBuild) NuGet package into your solution project (_.cdsproj_).

```shell
dotnet add package PowerVersion.MSBuild
```

## Usage

Simply build the solution project and the outputted solution's version will be set by PowerVersion based on the Git history.

### Version increments

This section describes the different mechanisms in place to increment the solution version for a commit.

The starting version for the calculation is either the version in the _Solution.xml_ (if no [version tag](#version-tags) is found) or the version in the latest version tag. It is recommended to set the version in the _Solution.xml_ to `0.0.0` to ensure that all versioning is handled by PowerVersion - if have an existing solution project, create a version tag to ensure version calculation is started at the correct version.

#### Metadata commits

Metadata commits are commits that make updates under the solution metadata directory. By default, this is the `src` folder in the project created by the Power Apps CLI with `pac solution init`.

These commits will automatically increment the solution version. The kind of increment depends on the commit message title and is based on [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/):

- A commit message with an '!' after the Conventional Commits type is a major increment (`refactor!: delete obsolete components`)
- A commit message with a Conventional Commits type of 'feat' is a minor increment (`feat: create an application`)
- A commit message containing anything else is a patch increment (`fix: contact form script error`)

#### Non-metadata commits

Non-metadata commits are commits that don't make any changes under the solution metadata directory but _do_ update files that are mapped into the solution at build-time with the Solution Packager. Typically, these will relate to code components such as web resources and plug-in assemblies. 

These kinds of commits do **not** automatically increment the solution version as it is not possible to determine which solutions have been updated. You must explicitly bump the solution manually using the following in the commit message body:

> +semver(\<solution-unique-name\>): \<increment\>

- An `<increment>` of `major` or `breaking` is a major increment
- An `<increment>` of `minor` or `feature` is a minor increment
- An `<increment>` of `patch` or `fix` is a patch increment

An explicit version increment of this type will take precedence if the same commit also increments the version as a metadata commit.

#### Version tags

It's possible to override the version calculated for a given commit using a Git tag:

> \<solution-unique-name\>/#.#.#

The version calculation will start at the most recent version tag for the solution (or the first commit if no tags are found). It is recommended to periodically create a version tag to improve the performance of the version calculation.

### Branch types

The version calculation differs based on the branch type.

#### Configuration

There are two MSBuild properties that can be set to configure how the branch type is determined for a given branch.

```xml
<PropertyGroup>
  <MainlineBranch>master</MainlineBranch>
  <ReleaseBranchPrefix>release/</ReleaseBranchPrefix>
</PropertyGroup>
```

#### Mainline branches

The mainline branch is `master` by default but this is configurable with the `MainlineBranch` MSBuild property.

The version of the mainline branch is calculated by determining the type of increment for each commit and applying each increment in order.

#### Release branches

A release branch is any branch that is prefixed by `release/` by default but this is configurable with the `ReleaseBranchPrefix` MSBuild property.

The version is calculated by taking the mainline version (for the commit the release branch is based on) and setting the revision part of the version to the total number of commits on the release branch that increment the solution version. Only release branches generate a revision version (e.g. `#.#.#.#` rather than `#.#.#`) and this avoids hotfix versions clashing with mainline versions.

#### Topic branches

A topic branch is a branch that is neither the mainline branch nor does it match the release branch prefix.

The version of a feature branch is calculated by taking the mainline version (for the commit the feature branch is based on) and incrementing this by the highest increment found on the feature branch.

## Contributing

Refer to the contributing [guide](./CONTRIBUTING.md).