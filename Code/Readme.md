# Kerosene Tools Launcher 7.4.1

This library contains utilities for launching test cases in an easy and convenient
way. This library is particulary useful to launch in a console the test cases that
have been built for the Visual Studio Unit Test framework.

-	Finds the test classes in the main project (those marked with the [TestClass]
attribute) and for each of them finds their test methods (as identified by the
[TestMethod] attribute). Once this list is constructed executes all of them.

-	When a method is marked with the [OnlyThisTest] attribute then only this method
is executed. This attribute is used to restrict the execution to the method of
interest in a given moment, despite how many test classes or methods have been found
in the project.

-	When this attribute maks several methods, even across diferent test classes,
only those methods are executed.
