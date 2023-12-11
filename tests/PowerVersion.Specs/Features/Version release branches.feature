Feature: Version release branches

Background:
	Given a Git repository has been initalised
	And a solution project has been created with the Power Apps CLI
	And the Power Version NuGet package has been installed
	And the master branch has received 1 or more commits
	And the currently calculated solution version is known

Scenario: Solution project is built on a release branch
	Given I have checked out a release branch
	And the release branch has received 1 or more commits
	When the solution project is built
	Then the version is the mainline version with a revision number equal to the count of incrementing commits on the release branch