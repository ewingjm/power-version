Feature: Increment versions with Conventional Commits

Background:
	Given a Git repository has been initalised
	And a solution project has been created with the Power Apps CLI
	And the Power Version NuGet package has been installed
	And the master branch has received 1 or more commits
	And the currently calculated solution version is known

Scenario: Solution project is built with a major version incrementing Conventional Commit
	Given a commit has been made with solution metadata updates and an '!' after the Conventional Commits type in the commit subject
	When the solution project is built
	Then the solution version is incremented to the next major version

Scenario: Solution project is built with a minor version incrementing Conventional Commit
	Given a commit has been made with solution metadata updates and a Conventional Commits type of 'feat'
	When the solution project is built
	Then the solution version is incremented to the next minor version

Scenario: Solution project is built with a patch version incrementing Conventional Commit
	Given a commit has been made with solution metadata updates and a Conventional Commits type in commit subject that is not 'feat'
	When the solution project is built
	Then the solution version is incremented to the next patch version