all: build

build:
	echo 'get deps'
	bash nuget install -solutionDir .
	echo 'compile modules'
	mcs @build.rsp
