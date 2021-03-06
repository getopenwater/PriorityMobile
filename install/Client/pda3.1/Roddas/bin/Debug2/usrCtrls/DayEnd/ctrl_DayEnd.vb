﻿Imports CPCL
Imports System.Xml
Imports PriorityMobile

Public Class ctrl_DayEnd
    Inherits iView

    Friend Enum ePrintWhat
        Cash = 1
        Returns = 2
    End Enum

    Private PrintWhat As ePrintWhat

#Region "Initialisation and Finalisation"

    Public Overrides ReadOnly Property MyControls() As ControlCollection
        Get
            Return Me.Controls
        End Get
    End Property

    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

    End Sub

    Public Overrides Sub FormClosing()
        MyBase.FormClosing()
    End Sub

#End Region

#Region "Overides Base Properties"

    'Public Overrides ReadOnly Property ButtomImage() As String
    '    Get
    '        Return "calendar.bmp"
    '    End Get
    'End Property


#End Region

#Region "Overrides base Methods"

    Public Overrides Sub Bind()

        If MsgBox("Print Cash Report?", MsgBoxStyle.OkOnly) = MsgBoxResult.Ok Then
            PrintWhat = ePrintWhat.Cash
            With thisForm.Printer
                If Not .Connected Then
                    .BeginConnect(thisForm.MACAddress)
                    Do While .WaitConnect
                        Threading.Thread.Sleep(100)
                    Loop
                End If
                If .Connected Then PrintForm()
            End With
        End If

        If MsgBox("Print Returns Report?", MsgBoxStyle.OkOnly) = MsgBoxResult.Ok Then
            PrintWhat = ePrintWhat.Returns
            With thisForm.Printer
                If Not .Connected Then
                    .BeginConnect(thisForm.MACAddress)
                    Do While .WaitConnect
                        Threading.Thread.Sleep(100)
                    Loop
                End If
                If .Connected Then PrintForm()
            End With
        End If

    End Sub

#End Region

#Region "Direct Activations"

    Public Overrides Sub DirectActivations(ByRef ToolBar As daToolbar)
        ToolBar.Add(AddressOf hCloseDelivery, "delete.BMP", True)
    End Sub

    Public Overrides Sub PrintForm()
        Select Case PrintWhat
            Case ePrintWhat.Cash
                PrintCash()
            Case ePrintWhat.Returns
                PrintReturns()
        End Select
    End Sub

    Public Sub PrintCash()
        Dim dv As XmlNode

        With thisForm
            dv = .FormData.SelectSingleNode("pdadata/home")



            Dim headerFont As New PrinterFont(50, 5, 2) 'variable width. 
            Dim largeFont As New PrinterFont(30, 0, 3) '16 
            Dim smallFont As New PrinterFont(35, 0, 2) '8 

            Using lblCashAnalysis As New Label(thisForm.Printer, eLabelStyle.receipt)

                Dim rcptHead As New ReceiptFormatter(64, _
                                                    New FormattedColumn(32, 0, eAlignment.Left), _
                                                    New FormattedColumn(32, 32, eAlignment.Left))
                rcptHead.AddRow("", "")
                rcptHead.AddRow("Date:", Now.ToString("dd/MM/yyyy"))
                rcptHead.AddRow("Route Number:", dv.SelectSingleNode("routenumber").InnerText)

                Dim rcptPayments As New ReceiptFormatter(64, _
                                                     New FormattedColumn(16, 0, eAlignment.Center), _
                                                     New FormattedColumn(16, 16, eAlignment.Center), _
                                                     New FormattedColumn(16, 32, eAlignment.Center), _
                                                     New FormattedColumn(16, 48, eAlignment.Center))

                Dim payments As XmlNodeList = dv.SelectNodes("//delivery/payment[cash!=0 or cheque!=0]")

                Dim cashTotal As Double = 0.0
                Dim chequeTotal As Double = 0.0

                rcptPayments.AddRow("Cust Code:", "Cust Name:", "Cash Amount:", "Cheque Amount:")

                For Each payment As XmlNode In payments
                    Dim cCode As String = payment.ParentNode.SelectSingleNode("customer/custnumber").InnerText
                    Dim cName As String = payment.ParentNode.SelectSingleNode("customer/custname").InnerText
                    Dim cash As Double = CDbl(payment.SelectSingleNode("cash").InnerText)
                    Dim cheque As Double = CDbl(payment.SelectSingleNode("cheque").InnerText)


                    rcptPayments.AddRow(cCode, cName, cash.ToString("c").Replace("£", "#"), cheque.ToString("c").Replace("£", "#"))
                    cashTotal += cash
                    chequeTotal += cheque
                Next




                'iterate through payments - nifty xpath needed.


                Dim rcptTotals As New ReceiptFormatter(64, _
                                                   New FormattedColumn(22, 0, eAlignment.Right), _
                                                   New FormattedColumn(21, 22, eAlignment.Right), _
                                                   New FormattedColumn(21, 43, eAlignment.Right))
                rcptTotals.AddRow("", "Total Cash Amount:", "Total Cheque Amount:")
                rcptTotals.AddRow("", cashTotal.ToString("c").Replace("£", "#"), chequeTotal.ToString("c").Replace("£", "#"))
                'calculated from the above data I suppose 
                rcptTotals.AddRow("", "", "Total Payments:")
                rcptTotals.AddRow("", "", (cashTotal + chequeTotal).ToString("c").Replace("£", "#"))

                Dim rcptCashAnalysis As New ReceiptFormatter(64, _
                                                         New FormattedColumn(32, 0, eAlignment.Left), _
                                                         New FormattedColumn(16, 32, eAlignment.Left), _
                                                         New FormattedColumn(16, 48, eAlignment.Left))
                'lots of rows - this is a written box. 

                rcptCashAnalysis.AddRow("", "Pounds:", "Pence:")
                rcptCashAnalysis.AddRow("50 Pound Notes", "", "")
                rcptCashAnalysis.AddRow("20 Pound Notes", "", "")
                rcptCashAnalysis.AddRow("10 Pound Notes", "", "")
                rcptCashAnalysis.AddRow("5 Pound Notes", "", "")
                rcptCashAnalysis.AddRow("2 Pound Coins", "", "")
                rcptCashAnalysis.AddRow("1 Pound Coins", "", "")
                rcptCashAnalysis.AddRow("50 Pence Coins", "", "")
                rcptCashAnalysis.AddRow("20 Pence Coins", "", "")
                rcptCashAnalysis.AddRow("Silver", "", "")
                rcptCashAnalysis.AddRow("Bronze", "", "")
                rcptCashAnalysis.AddRow("", "", "")
                rcptCashAnalysis.AddRow("", "Total Cash:", cashTotal.ToString("c").Replace("£", "#")) 'is this a calculated column? Yes, apparently. 
                rcptCashAnalysis.AddRow("", "", "")
                rcptCashAnalysis.AddRow("Cheque:", "", "")
                rcptCashAnalysis.AddRow("Other:", "", "")
                'are these calculated columns? Yes, apparently.
                rcptCashAnalysis.AddRow("", "Total Cheques:", "#" & chequeTotal)
                rcptCashAnalysis.AddRow("", "Grand Total:", (cashTotal + chequeTotal).ToString("c").Replace("£", "#"))
                'though I wager these will want changing later when the drivers don't actually check their totals.


                With lblCashAnalysis
                    .CharSet(eCountry.UK)
                    'logo
                    .AddImage("roddas.pcx", New Point(186, thisForm.Printer.Dimensions.Height + 10), 147)

                    'line
                    .AddLine(New Point(10, thisForm.Printer.Dimensions.Height + 10), _
                             New Point(thisForm.Printer.Dimensions.Width - 10, thisForm.Printer.Dimensions.Height + 10), 10, 15)

                    'header = 222.5px wide
                    .AddText("PAYMENTS RECEIVED", New Point((thisForm.Printer.Dimensions.Width / 2) - 223, thisForm.Printer.Dimensions.Height + 10), _
                             headerFont)

                    .AddLine(New Point(10, thisForm.Printer.Dimensions.Height + 10), _
                             New Point(thisForm.Printer.Dimensions.Width - 10, thisForm.Printer.Dimensions.Height + 10), 10, 15)
                    'date & routenumber

                    For Each StrVal In rcptHead.FormattedText
                        .AddText(StrVal, New Point(22, thisForm.Printer.Dimensions.Height), smallFont)
                    Next

                    'line
                    .AddLine(New Point(10, thisForm.Printer.Dimensions.Height + 10), _
                             New Point(thisForm.Printer.Dimensions.Width - 10, thisForm.Printer.Dimensions.Height + 10), 2)

                    'payments rcpt
                    For Each StrVal In rcptPayments.FormattedText
                        .AddText(StrVal, New Point(22, thisForm.Printer.Dimensions.Height), smallFont)
                    Next

                    'totals
                    .AddLine(New Point(10, thisForm.Printer.Dimensions.Height + 10), _
                                             New Point(thisForm.Printer.Dimensions.Width - 10, thisForm.Printer.Dimensions.Height + 10), 1)


                    For i As Integer = 0 To 1
                        .AddText(rcptTotals.FormattedText(i), New Point(22, thisForm.Printer.Dimensions.Height), smallFont)
                    Next
                    .AddLine(New Point(225, thisForm.Printer.Dimensions.Height + 10), _
                                            New Point(thisForm.Printer.Dimensions.Width - 10, thisForm.Printer.Dimensions.Height + 10), 1)

                    For i As Integer = 2 To 3
                        .AddText(rcptTotals.FormattedText(i), New Point(22, thisForm.Printer.Dimensions.Height), smallFont)
                    Next

                    .AddLine(New Point(420, thisForm.Printer.Dimensions.Height + 10), _
                                            New Point(thisForm.Printer.Dimensions.Width - 10, thisForm.Printer.Dimensions.Height + 10), 1)

                    'line

                    .AddLine(New Point(10, thisForm.Printer.Dimensions.Height + 10), _
                             New Point(thisForm.Printer.Dimensions.Width - 10, thisForm.Printer.Dimensions.Height + 10), 10)

                    .AddText("CASH ANALYSIS", New Point((thisForm.Printer.Dimensions.Width / 2) - 166, thisForm.Printer.Dimensions.Height + 10), _
                             headerFont)

                    .AddLine(New Point(10, thisForm.Printer.Dimensions.Height + 10), _
                             New Point(thisForm.Printer.Dimensions.Width - 10, thisForm.Printer.Dimensions.Height + 10), 10)
                    'cash

                    .AddText(rcptCashAnalysis.FormattedText(0), New Point(22, thisForm.Printer.Dimensions.Height), smallFont)

                    .AddLine(New Point(10, thisForm.Printer.Dimensions.Height + 10), _
                             New Point(thisForm.Printer.Dimensions.Width - 10, thisForm.Printer.Dimensions.Height + 10), 1)

                    For i As Integer = 1 To 10
                        .AddText(rcptCashAnalysis.FormattedText(i), New Point(22, thisForm.Printer.Dimensions.Height), smallFont)
                    Next

                    .AddLine(New Point(10, thisForm.Printer.Dimensions.Height + 10), _
                             New Point(thisForm.Printer.Dimensions.Width - 10, thisForm.Printer.Dimensions.Height + 10), 1)

                    .AddText(rcptCashAnalysis.FormattedText(12), New Point(22, thisForm.Printer.Dimensions.Height), smallFont)

                    .AddLine(New Point(280, thisForm.Printer.Dimensions.Height + 10), _
                             New Point(thisForm.Printer.Dimensions.Width - 10, thisForm.Printer.Dimensions.Height + 10), 1)

                    For i As Integer = 14 To 15
                        .AddText(rcptCashAnalysis.FormattedText(i), New Point(22, thisForm.Printer.Dimensions.Height), smallFont)
                    Next

                    .AddLine(New Point(10, thisForm.Printer.Dimensions.Height + 10), _
                             New Point(thisForm.Printer.Dimensions.Width - 10, thisForm.Printer.Dimensions.Height + 10), 1)


                    .AddText(rcptCashAnalysis.FormattedText(16), New Point(22, thisForm.Printer.Dimensions.Height), smallFont)


                    .AddLine(New Point(280, thisForm.Printer.Dimensions.Height + 10), _
                            New Point(thisForm.Printer.Dimensions.Width - 10, thisForm.Printer.Dimensions.Height + 10), 1)

                    .AddText(rcptCashAnalysis.FormattedText(17), New Point(22, thisForm.Printer.Dimensions.Height), smallFont)

                    .AddLine(New Point(280, thisForm.Printer.Dimensions.Height + 10), _
                            New Point(thisForm.Printer.Dimensions.Width - 10, thisForm.Printer.Dimensions.Height + 10), 1)

                    .AddLine(New Point(10, thisForm.Printer.Dimensions.Height + 10), _
                             New Point(thisForm.Printer.Dimensions.Width - 10, thisForm.Printer.Dimensions.Height + 10), 10)

                    'tear 'n' print!
                    .AddTearArea(New Point(0, thisForm.Printer.Dimensions.Height))
                    thisForm.Printer.Print(.toByte)


                End With
            End Using

        End With
    End Sub

    Public Sub PrintReturns()

        With thisForm

            Dim headerFont As New PrinterFont(50, 5, 2) 'variable width. 
            Dim largeFont As New PrinterFont(30, 0, 3) '16 
            Dim smallFont As New PrinterFont(35, 0, 2) '8 

            Using lblReturnsAnalysis As New Label(thisForm.Printer, eLabelStyle.receipt)

                Dim rcptReStock As New ReceiptFormatter(64, _
                                                    New FormattedColumn(10, 0, eAlignment.Left), _
                                                    New FormattedColumn(22, 10, eAlignment.Left), _
                                                    New FormattedColumn(6, 32, eAlignment.Left), _
                                                    New FormattedColumn(26, 38, eAlignment.Left))

                Dim returns As XmlNodeList = .FormData.SelectNodes("//creditnote/parts/part[reason[not(contains(., ""Faulty Goods""))] and qty!=0 and rcvdqty!=0]")

                rcptReStock.AddRow("Part:", "Part Desc:", "Qty:", "Customer Reason:")


                For Each pReturn As XmlNode In returns
                    Dim name As String = pReturn.SelectSingleNode("name").InnerText
                    Dim des As String = pReturn.SelectSingleNode("des").InnerText
                    Dim qty As String = pReturn.SelectSingleNode("rcvdqty").InnerText
                    Dim reason As String = pReturn.SelectSingleNode("reason").InnerText
                    rcptReStock.AddRow(name, des, qty, reason)
                Next

                Dim rcptQuarantine As New ReceiptFormatter(64, _
                                                    New FormattedColumn(10, 0, eAlignment.Left), _
                                                    New FormattedColumn(22, 10, eAlignment.Left), _
                                                    New FormattedColumn(6, 32, eAlignment.Center), _
                                                    New FormattedColumn(26, 38, eAlignment.Left))

                rcptQuarantine.AddRow("Part:", "Part Desc:", "Qty:", "Customer Reason:")

                Dim disposals As XmlNodeList = .FormData.SelectNodes("//creditnote/parts/part[reason[contains(., ""Faulty Goods"")] and qty!=0 and rcvdqty!=0]")

                For Each disposal As XmlNode In disposals
                    Dim name As String = disposal.SelectSingleNode("name").InnerText
                    Dim des As String = disposal.SelectSingleNode("des").InnerText
                    Dim qty As String = disposal.SelectSingleNode("qty").InnerText
                    Dim reason As String = disposal.SelectSingleNode("reason").InnerText

                    rcptQuarantine.AddRow(name, des, qty, reason)
                Next

                With lblReturnsAnalysis
                    .CharSet(eCountry.UK)
                    'logo
                    .AddImage("roddas.pcx", New Point(186, thisForm.Printer.Dimensions.Height + 10), 147)

                    'line
                    .AddLine(New Point(10, thisForm.Printer.Dimensions.Height + 10), _
                             New Point(thisForm.Printer.Dimensions.Width - 10, thisForm.Printer.Dimensions.Height + 10), 10, 15)

                    'header = 204.5px wide
                    .AddText("RETURNS ANALYSIS", New Point((thisForm.Printer.Dimensions.Width / 2) - 205, thisForm.Printer.Dimensions.Height + 10), _
                             headerFont)

                    .AddLine(New Point(10, thisForm.Printer.Dimensions.Height + 10), _
                             New Point(thisForm.Printer.Dimensions.Width - 10, thisForm.Printer.Dimensions.Height + 10), 10, 15)


                    .AddText("Returned Goods for Re-Stock", New Point(20, thisForm.Printer.Dimensions.Height + 30), _
                             largeFont)

                    .AddTearArea(New Point(0, thisForm.Printer.Dimensions.Height), 15)

                    For Each strval In rcptReStock.FormattedText
                        .AddText(strval, New Point(22, thisForm.Printer.Dimensions.Height), smallFont)

                    Next

                    .AddLine(New Point(10, thisForm.Printer.Dimensions.Height + 10), _
                             New Point(thisForm.Printer.Dimensions.Width - 10, thisForm.Printer.Dimensions.Height + 10), 2, 15)

                    .AddText("Returned Goods for Quarantine", New Point(20, thisForm.Printer.Dimensions.Height + 20), _
                                 largeFont)

                    .AddTearArea(New Point(0, thisForm.Printer.Dimensions.Height), 15)

                    For Each strval In rcptQuarantine.FormattedText
                        .AddText(strval, New Point(22, thisForm.Printer.Dimensions.Height), smallFont)
                    Next


                    .AddLine(New Point(10, thisForm.Printer.Dimensions.Height + 10), _
                             New Point(thisForm.Printer.Dimensions.Width - 10, thisForm.Printer.Dimensions.Height + 10), 10, 15)


                    'tear 'n' print!
                    .AddTearArea(New Point(0, thisForm.Printer.Dimensions.Height))
                    thisForm.Printer.Print(.toByte)

                End With
            End Using

        End With
    End Sub

    Private Sub hCloseDelivery()

        With thisForm.FormData
            If MsgBox("Close and post?", MsgBoxStyle.OkCancel) = MsgBoxResult.Ok Then
                Cursor.Current = Cursors.WaitCursor
                .SelectSingleNode("pdadata").Attributes.Append(xmlForms.postAttribute)
                Dim postExcep As Exception = Nothing

                If Not xmlForms.FormData.Post(postExcep) Then
                    Cursor.Current = Cursors.Default
                    MsgBox(postExcep.Message, MsgBoxStyle.Critical)

                Else
                    Cursor.Current = Cursors.Default
                    MsgBox("Data posted. Closing Application.")
                    xmlForms.FormData.DeleteLocalCache()
                    Application.Exit()

                    'Dim changednodes As XmlNodeList = xmlForms.FormData.Document.SelectNodes("//*[@changed ='1']")
                    'While changednodes.Count > 0
                    '    changednodes(0).Attributes.RemoveNamedItem("changed")
                    '    thisForm.Save()
                    '    changednodes = .SelectNodes("//*[@changed ='1']")
                    'End While

                    'With thisForm
                    '    .TopForm("Home").CloseForm()
                    '    .TopForm("Home").CloseForm()
                    '    .TopForm("Home").CurrentForm.Views(0).RefreshData()
                    '    .RefreshForm()
                    '    'xmlForms.FormData.Sync()
                    '    xmlForms.FormData.DeleteLocalCache()
                    '    prioritymobile.formdata.
                    'End With

                End If
            End If

        End With

    End Sub

#End Region

End Class
