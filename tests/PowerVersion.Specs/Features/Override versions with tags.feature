Feature: Override versions with tags

Background:
	Given a Git repository has been initalised
	And a solution project has been created with the Power Apps CLI
	And the PowerVersion NuGet package has been installed
	And the master branch has received 1 or more commits

Scenario: Solution project is built on a commit with a solution version tag
	And a tag has been made matching the format `<solution>/x.x.x`
	When the solution project is built
	Then the solution version matches the version in the tag