all: build

build:
	mozroots --import --sync
	echo 'get deps'
	bash nuget install ./src/Owin.Routing/packages.config -solutionDir ./
	bash nuget install ./src/Tests/packages.config -solutionDir ./
	echo 'compile bits for testing'
	dmcs /define:NUNIT /out:Owin.Routing.Tests.dll @build.rsp
	dmcs @tests.rsp
	echo 'run tests'
	nunit-console Owin.Routing.Tests.dll Tests.dll
	echo 'compile release bits'
	dmcs /out:Owin.Routing.dll @build.rsp
