#!/usr/bin/env bash
# -*- coding: utf-8 -*-

set -eu
set -o pipefail

dotnet build FSharp.Codecs.Redis.sln -c Release
dotnet pack FSharp.Codecs.Redis -c Release