Feature: Version feature branches

Background:
	Given a Git repository has been initalised
	And a solution project has been created with the Power Apps CLI
	And the Power Version NuGet package has been installed
	And the master branch has received 1 or more commits
	And the currently calculated solution version is known

Scenario: Solution project is built on a feature branch
	Given I have checked out a feature branch
	And the feature branch has received 1 or more commits
	When the solution project is built
	Then the version is the mainline version incremented by the highest version increment on the feature branch