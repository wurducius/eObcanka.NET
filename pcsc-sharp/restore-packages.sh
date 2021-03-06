#!/bin/bash

root=$(dirname "$0")
dir=$root/.paket
paket=$dir/paket.exe

maybe_mono=mono
if [ "$OS" == "Windows_NT" ]; then
  # Use .NET on Windows
  maybe_mono=
fi

$maybe_mono "$paket" restore
exit_code=$?
if [ $exit_code -ne 0 ]; then
  exit $exit_code
fi
