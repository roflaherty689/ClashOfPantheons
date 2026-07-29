.PHONY: build build-no-archive

build:
	cmd.exe /c Build-Windows.cmd

build-no-archive:
	cmd.exe /c Build-Windows.cmd -SkipArchive
