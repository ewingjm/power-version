# Power Version![](./docs/images/logo.svg)
## Table of contents

- [Power Version](#power-version)
  - [Table of contents](#table-of-contents)
  - [Introduction](#introduction)
  - [Prerequisites](#prerequisites)
  - [Installation](#installation)
  - [Usage](#usage)
    - [Version incrementing](#version-incrementing)
      - [Solution metadata commits](#solution-metadata-commits)
      - [Code component commits](#code-component-commits)
      - [Version tags](#version-tags)
    - [Branch types](#branch-types)
      - [Mainline](#mainline)
      - [Feature](#feature)
      - [Release](#release)
  - [Contributing](#contributing)


## Introduction

Generating a [Semantic Version](https://semver.org/) number from Git history can often be advantageous. A tool commonly used to do this is [GitVersion](https://gitversion.net/docs/). GitVersion works great in most scenarios but, unfortunately, Power Apps solutions pose some unique challenges:

- More than one solution will often be source-controlled in the same repository (e.g. as part of a package)
- Solution versions do not accept all valid Semantic Versions (e.g. those with suffixes)

This package has been created to enable Power Apps developer to benefit from the functionality offered by tools like GitVersion while addressing the above challenges.

## Prerequisites

- You must have a _.cdsproj_ solution project created with the Power Apps CLI (e.g. `pac solution init`)
- You must be source-controlling your solution project in a Git repository
- You must be following trunk-based Git workflow similar to what is described in this Microsoft Developer Blogs [post](https://devblogs.microsoft.com/devops/release-flow-how-we-do-branching-on-the-vsts-team/)
  - Developers branch off and merge into a trunk (e.g. `master` or `main`)
  - Releases are supported using release branches

## Installation

Install the [PowerVersion.MSBuild](https://www.nuget.org/packages/PowerVersion.MSBuild) NuGet package into your solution project (_.cdsproj_).

## Usage

Simply build the solution project after installing the NuGet package. The outputted solution file will have a version set which has been derived from the Git history. It is recommended to set your solution version in source control to `0.0.0` to ensure that all versioning is handled by Power Version. 

If you are adopting Power Version for an existing solution project, refer to the [Version tags](#version-tags) section for information on overriding version to start the calculation from.

See below for details on how versions are calculated.

### Version incrementing

There are different mechanisms in place to increment a solution version with a commit.

#### Solution metadata commits

Solution metadata commits are commits that make updates under the solution metadata directory. By default, this is the `src` folder in the project created by the Power Apps CLI with `pac solution init`.

These kinds of commits will automatically bump the solution version. The kind of bump depends on the commit title and is based on [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/):

- A commit message with an '!' after the Conventional Commits type is a major increment (`refactor!: delete obsolete components`)
- A commit message with a Conventional Commits type of 'feat' is a minor increment (`feat: create an application`)
- A commit message containing anything else is a patch increment (`fix: contact form script error`)

#### Code component commits

Code component commits are commits that don't make any changes under the solution metadata directory but do update files that are mapped into the solution at build-time with the Solution Packager. Typically, these will relate to code components such as web resources and plug-in assemblies. 

These kinds of commits do **not** automatically increment the solution version as it is not possible to determine which solutions have been updated. You must explicitly bump the solution manually using the following in the commit message body. 

`+semver(<solution-unique-name>): <increment>`

- An `<increment>` of `major` or `breaking` is a major increment
- An `<increment>` of `minor` or `feature` is a minor increment
- An `<increment>` of `patch` or `fix` is a patch increment

Note that an explicit version increment of this type will take precedence if the same commit also increments the version as a metadata commit.

#### Version tags

It's possible to override the version calculated for a given commit using Git tags. 

`<solution-unique-name>/#.#.#`

The version calculation will start at the most recent version tag for the solution (or the first commit if no tags are found).

It is recommended to periodically create a version tag to improve the performance of the version calculation.

### Branch types

Versions are calculated differently depending on the type of branch.

#### Mainline

The mainline branch is `master` by default. This is configurable by setting a `MainlineBranch` MSBuild property.

The version is calculated for the mainline branch according to the rules defined in [Version incrementing](#version-incrementing).

#### Feature

A feature branch is any branch that is neither the mainline nor a release branch.

The version is calculated by taking the mainline version (for the commit the feature branch is based on) and incrementing this by the highest increment found on all of the feature branch commits.

#### Release

A release branch is any branch that is prefixed by `release/`. This is configurable by setting a `ReleaseBranchPrefix` MSBuild property.

The version is calculated by taking the mainline version (for the commit the release branch is based on) and setting the revision part of the version to the total count of incrementing commits on the release branch.

Only a release branch will generate a revision version (e.g. `#.#.#.#` rather than `#.#.#`). This avoids hotfix versions clashing with mainline versions.

## Contributing

Refer to the contributing [guide](./CONTRIBUTING.md).