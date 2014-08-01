@echo off
if exist bin (rd /s /q bin)
if exist obj (rd /s /q obj)
pushd .
set root=%~dp0
for /f %%f in ('dir . /b /ad') do (
  cd %root%%%f
  if exist bin (rd /s /q bin)
  if exist obj (rd /s /q obj)
  for /f %%f in ('dir . /b /ad') do (
    if exist %%f\bin (rd /s /q %%f\bin)
    if exist %%f\obj (rd /s /q %%f\obj)
  )
)
popd