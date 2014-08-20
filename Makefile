all: build

build:
	mozroots --import --sync
	echo 'get deps'
	bash nuget install ./src/Owin.Routing/packages.config -solutionDir ./
	bash nuget install ./src/Tests/packages.config -solutionDir ./
	mkdir -p .out
	echo 'compile release bits'
	mcs /out:.out/Owin.Routing.dll @build.rsp
	echo 'compile tesing bits'
	mcs /define:NUNIT /out:.out/Owin.Routing.Tests.dll @build.rsp
	mcs /out:.out/Tests.dll @tests.rsp
	echo 'run tests'
	cp packages/NUnit.2.6.3/lib/nunit.framework.dll .out/
	nunit-console .out/Owin.Routing.Tests.dll .out/Tests.dll

