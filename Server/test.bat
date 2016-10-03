@echo off

:: Build Tests
echo building tests...
javac -cp "./test;./bin;./jar/boon.jar;./jar/junit.jar" test/TestSuite.java
echo built!

:: Run Tests
echo running tests...
java -cp "./test;./bin/;./jar/boon.jar;./jar/junit.jar;./jar/hamcrest.jar" org.junit.runner.JUnitCore TestSuite
echo testing complete
