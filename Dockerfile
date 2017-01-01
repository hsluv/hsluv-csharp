FROM ubuntu:15.04

RUN apt-get update
RUN apt-get install -y mono-devel

ADD . /hsluv
WORKDIR /hsluv

RUN mcs -target:library Hsluv/Hsluv.cs
RUN mcs HsluvTest/HsluvConverterTest.cs HsluvTest/MiniJSON.cs Hsluv/Hsluv.cs -resource:HsluvTest/Resources/JsonSnapshotRev3.txt,JsonSnapshotRev3 -main:HsluvTest.HsluvConverterTest
RUN mono ./HsluvTest/HsluvConverterTest.exe
