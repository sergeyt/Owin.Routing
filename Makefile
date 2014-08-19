all: build

build:
	echo 'get deps'
	bash nuget install -solutionDir .
	echo 'compile bits for testing'
	mcs /define:NUNIT /out:Owin.Routing.Tests.dll @build.rsp
	mcs @tests.rsp
	echo 'run tests'
	nunit-console Owin.Routing.Tests.dll Tests.dll
	echo 'compile release bits'
	mcs /out:Owin.Routing.dll @build.rsp
