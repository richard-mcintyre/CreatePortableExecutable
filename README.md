# Create Portable Executable

Creates a 32bit MS Windows executable given a flat code file.

## Usage
Create a json file describing the code and data sections and any MS Window APIs the code will call.  For example:
```json
{
    "Code": {
      "AddressRVA": "0x1000",
      "Content": "ConsoleWrite.bin"
    },
  
    "Data": {
      "AddressRVA": "0x2000",
      "Content": "ConsoleWrite.data"
    },
  
    "Imports": {
      "AddressRVA": "0x3000",
      "Libraries": [
        {
          "Name": "KERNEL32.dll",
          "Functions": [
            "GetCurrentProcess",
            "GetStdHandle",
            "Sleep",
            "TerminateProcess",
            "WriteFile"
          ]
        }
      ]
    }
}
```

The `AddressRVA` value is the address the section will be loaded at (after its added to the image base).

Create a file containing the contents of the data section. In this example the file is called `ConsoleWrite.data`.

Write the program in assembler.  For example:
```asm
; ------------------------------------------------------------
; Imported functions
GetCurrentProcess                       equ     0x403000
GetStdHandle                            equ     0x403004
Sleep                                   equ     0x403008
TerminateProcess                        equ     0x40300c
WriteFile                               equ     0x403010

; ------------------------------------------------------------
; Location of data in the data section (ConsoleWrite.data)
Message             equ 0x402000
MessageLength       equ 0x402020
StdOutHandle        equ 0x402024        ; Set by code
BytesWritten        equ 0x402028        ; Set by WriteFile

; ------------------------------------------------------------
; Win32 Constants
STD_OUTPUT_HANDLE   equ -11

; ------------------------------------------------------------

bits 32
org 401000h

; --------------------
; Get stdout handle
push STD_OUTPUT_HANDLE
call [GetStdHandle]
mov [StdOutHandle], eax     ; Save the std handle

; --------------------
; Write a message 3 times every 500ms
xor ebx, ebx                ; ebx = 0

loop:
push 0                      ; lpOverlapped
push dword BytesWritten     ; lpNumberOfBytesWritten
push dword [MessageLength]  ; nNumberOfBytesToWrite
push dword Message          ; lpBuffer
push dword [StdOutHandle]   ; hFile (stdout)
call [WriteFile]

push 500                    ; dwMilliseconds
call [Sleep]

inc ebx
cmp ebx, 3
jle loop

; --------------------
; Terminate the process
call [GetCurrentProcess]
push 1                      ; Exit code
push eax                    ; Handle of process (return value of GetCurrentProcess)
call [TerminateProcess]
```

Build the code into a flat binary.  For example, with NASM:
```
nasm -f bin ConsoleWrite.asm -o ConsoleWrite.bin
```

Then use CreatePE to create the executable file:
```
CreatePE ConsoleWrite.json ConsoleWrite.exe
```

## Determining the address of imported functions
The address of the imported functions are only known after `CreatePE` is executed.

`CreatePE` outputs the addresses when it is run.