﻿Imports Microsoft.VisualBasic
Imports System.Collections.Generic
Imports System.Xml
Imports System.IO
Imports System.Web
Imports System.Web.UI.WebControls

Public Class CartItem

#Region "Initialisations"

    Public Sub New()
        MyBase.new()
    End Sub

    Public Sub New(ByVal PARTNAME As String, ByVal PARTDES As String, ByVal PARTPRICE As String, ByVal QTY As String, ByVal PACKFAMILY As Integer, ByVal SALESTAX As String, ByVal REFERER As String)
        MyBase.new()
        _PackFamily = PACKFAMILY
        _PARTNAME = PARTNAME
        _PARTDES = PARTDES
        _PARTPRICE = PARTPRICE
        _QTY = QTY
        _SALESTAX = SALESTAX
        _REFERER = REFERER
    End Sub

#End Region

#Region "Public Properties"

    Private _PARTNAME As String = ""
    Public Property PARTNAME() As String
        Get
            Return _PARTNAME
        End Get
        Set(ByVal value As String)
            _PARTNAME = value
        End Set
    End Property

    Private _PARTDES As String = ""
    Public Property PARTDES() As String
        Get
            Return _PARTDES
        End Get
        Set(ByVal value As String)
            _PARTDES = value
        End Set
    End Property

    Private _PARTPRICE As String = ""
    Public Property PARTPRICE() As String
        Get
            Return FormatDouble(_PARTPRICE)
        End Get
        Set(ByVal value As String)
            _PARTPRICE = value
        End Set
    End Property

    Private _QTY As String = ""
    Public Property QTY() As String
        Get
            Return _QTY
        End Get
        Set(ByVal value As String)
            _QTY = value
        End Set
    End Property

    Private _SALESTAX As String = ""
    Public Property SALESTAX() As String
        Get
            Return FormatDouble(_SALESTAX)
        End Get
        Set(ByVal value As String)
            _SALESTAX = value
        End Set
    End Property

    Private _LINETOTAL As String = ""
    Public ReadOnly Property LINETOTAL() As String
        Get
            Return FormatDouble(CStr(CInt(_QTY) * (CDbl(_PARTPRICE))))
        End Get
    End Property

    Private _PackFamily As Integer
    Public Property PackFamily() As Integer
        Get
            Return _PackFamily
        End Get
        Set(ByVal value As Integer)
            _PackFamily = value
        End Set
    End Property

    Private _REFERER As String = ""
    Public Property REFERER() As String
        Get
            Return _REFERER
        End Get
        Set(ByVal value As String)
            _REFERER = value
        End Set
    End Property

#End Region

#Region "Private Functions"

    Private Function FormatDouble(ByVal str As String) As String
        If InStr(str, ".") Then
            Return Split(str, ".")(0) & "." & Left(Split(str, ".")(1) & "00", 2)
        Else
            Return str & ".00"
        End If
    End Function

#End Region

End Class