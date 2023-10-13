@echo off
nasm -f bin ConsoleWrite.asm -o ConsoleWrite.bin -l ConsoleWrite.lst
..\..\src\bin\debug\net7.0\CreatePE ConsoleWrite.json ConsoleWrite.exe