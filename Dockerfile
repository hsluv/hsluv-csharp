FROM ubuntu:15.04

RUN apt-get update
RUN apt-get install -y mono-devel
RUN apt-get install -y nuget
RUN apt-get install -y nunit-console
RUN mozroots --import --sync

ADD . /husl
WORKDIR /husl

RUN xbuild /p:Configuration=Release HUSL.sln
RUN nunit-console ./HUSLTest/bin/Release/HUSLTest.dll
