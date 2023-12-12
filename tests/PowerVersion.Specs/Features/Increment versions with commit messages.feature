Feature: Increment versions with commit messages

Background:
	Given a Git repository has been initalised
	And a solution project has been created with the Power Apps CLI
	And the PowerVersion NuGet package has been installed
	And the master branch has received 1 or more commits
	And the currently calculated solution version is known

Scenario: Solution project is built with a major version incrementing commit message
	Given a commit has been made with '+semver(<solutionName>): ' followed by any of the following in the commit message:
		| Increment type |
		| breaking       |
		| major          |
	When the solution project is built
	Then the solution version is incremented to the next major version
	
Scenario: Solution project is built with a minor version incrementing commit message
	Given a commit has been made with '+semver(<solutionName>): ' followed by any of the following in the commit message:
		| Increment type |
		| feature        |
		| minor          |
	When the solution project is built
	Then the solution version is incremented to the next minor version

Scenario: Solution project is built with a patch version incrementing commit message
	Given a commit has been made with '+semver(<solutionName>): ' followed by any of the following in the commit message:
		| Increment type |
		| fix            |
		| patch          |
	When the solution project is built
	Then the solution version is incremented to the next patch version