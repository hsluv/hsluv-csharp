FROM ubuntu:15.04

RUN apt-get update
RUN apt-get install -y mono-devel
RUN apt-get install -y nuget
RUN apt-get install -y nunit-console
RUN mozroots --import --sync

ADD . /husl
WORKDIR /husl

RUN xbuild /p:Configuration=Release Hsluv.sln
RUN nunit-console ./HsluvTest/bin/Release/HsluvTest.dll
