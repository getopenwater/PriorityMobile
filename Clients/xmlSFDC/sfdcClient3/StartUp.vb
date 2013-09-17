﻿Imports System.Runtime.InteropServices
Imports System.IO
Imports PrioritySFDC

Module StartUp

    Public StartFlags As New StartFlags

#Region " API Declarations "

    Public Const SW_HIDE As Integer = &H0
    Public Const SW_SHOW As Integer = &H1

    <DllImport("coredll.dll", CharSet:=CharSet.Auto)> _
    Public Function FindWindow(ByVal lpClassName As String, ByVal lpWindowName As String) As IntPtr
    End Function

    <DllImport("coredll.dll", CharSet:=CharSet.Auto)> _
    Public Function ShowWindow(ByVal hwnd As IntPtr, ByVal nCmdShow As Integer) As Boolean
    End Function

    <DllImport("coredll.dll", CharSet:=CharSet.Auto)> _
    Public Function EnableWindow(ByVal hwnd As IntPtr, ByVal enabled As Boolean) As Boolean
    End Function

    <DllImport("coredll.dll", SetLastError:=True)> _
    Public Function GetCapture() As IntPtr
    End Function

    <DllImport("aygshell.dll")> _
    Public Function SHFullScreen(ByVal hwndRequester As IntPtr, ByVal dwState As Integer) As Integer
    End Function

    Public Enum SHFS As Integer
        SHOWTASKBAR = &H1
        HIDETASKBAR = &H2
        SHOWSIPBUTTON = &H4
        HIDESIPBUTTON = &H8
        SHOWSTARTICON = &H10
        HIDESTARTICON = &H20
    End Enum

#End Region

    Public Sub Main(ByVal args As String())

        Dim arg As New clArg(args)

        Try
            With arg.Keys
                If .Contains("clearcache") Then
                    StartFlags.ClearCache = True
                End If
                If .Contains("wipedata") Then
                    StartFlags.WipeData = True
                End If
                If .Contains("noprovision") Then
                    StartFlags.NoProvision = True
                End If
                If .Contains("handler") Then
                    If File.Exists(arg("handler")) Then
                        ue = New UserEnv("PrioritySFDC", arg("handler"))
                    Else
                        Throw New Exception(String.Format("Local Handler {0} file not found.", arg("handler")))
                    End If
                Else
                    ue = New UserEnv("PrioritySFDC", "cHandler.dll")
                End If
            End With

            With ue
                .SaveProvision = Not (StartFlags.NoProvision)
                .Add("forms", _
                    New OfflineXML( _
                       ue, "sfdc.xml", "sfdc.ashx", _
                            New StartFlags( _
                                StartFlags.ClearCache, _
                                StartFlags.WipeData, _
                                StartFlags.NoProvision _
                            ) _
                    ) _
                )

            End With

            Application.Run(Menu)

        Catch ex As Exception
            Using sw As New StreamWriter("appcrash.txt", True)
                sw.WriteLine("{0}: {1}", Now.ToString, ex.Message)
                sw.Write(ex.StackTrace)
            End Using
            MsgBox(ex.Message, MsgBoxStyle.Critical + MsgBoxStyle.OkOnly, "Fatal Error.")
            Application.Exit()
        End Try

    End Sub

    Private _ue As UserEnv
    Public Property ue() As UserEnv
        Get
            Return _ue
        End Get
        Set(ByVal value As UserEnv)
            _ue = value
        End Set
    End Property

End Module


