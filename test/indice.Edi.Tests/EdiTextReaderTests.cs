﻿using indice.Edi.Tests.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace indice.Edi.Tests
{
    public class EdiTextReaderTests
    {
        private static readonly Assembly _assembly = typeof(EdiTextReaderTests).GetTypeInfo().Assembly;
        private static Stream GetResourceStream(string fileName) {
            var qualifiedResources = _assembly.GetManifestResourceNames().OrderBy(x => x).ToArray();
            Stream stream = _assembly.GetManifestResourceStream("indice.Edi.Tests.Samples." + fileName);
            return stream;
        }

        private static MemoryStream StreamFromString(string value) {
            return new MemoryStream(Encoding.UTF8.GetBytes(value ?? ""));
        }

        [Fact]
        public void ReaderTest() {
            var msgCount = 0;
            var grammar = EdiGrammar.NewTradacoms();

            using (var ediReader = new EdiTextReader(new StreamReader(GetResourceStream("tradacoms.order9.edi")), grammar)) {
                while (ediReader.Read()) {
                    if (ediReader.IsStartMessage) {
                        msgCount++;
                    }
                }
            }
            Assert.Equal(4, msgCount);
        }

        [Fact]
        public void DeserializeTest() {
            var grammar = EdiGrammar.NewTradacoms();
            var interchange = default(Interchange);
            using (var stream = GetResourceStream("tradacoms.utilitybill.edi")) {
                interchange = new EdiSerializer().Deserialize<Interchange>(new StreamReader(stream), grammar);
            }
            Assert.Equal(1, interchange.Invoices.Count);
        }

        [Fact]
        public void EscapeCharactersTest() {
            var grammar = EdiGrammar.NewTradacoms();
            var interchange = default(Interchange);
            using (var stream = GetResourceStream("tradacoms.utilitybill.escape.edi")) {
                interchange = new EdiSerializer().Deserialize<Interchange>(new StreamReader(stream), grammar);
            }
            Assert.Equal("GEORGE'S FRIED CHIKEN + SONS. Could be the best chicken yet?", interchange.Head.ClientName);
        }

        [Fact]
        public void EdiFact_01_Test()
        {
            var grammar = EdiGrammar.NewEdiFact();
            var interchange = default(Models.EdiFact01.Interchange);
            using (var stream = GetResourceStream("edifact.01.edi"))
            {
                interchange = new EdiSerializer().Deserialize<Models.EdiFact01.Interchange>(new StreamReader(stream), grammar);
            }

            //Test Interchange de-serialization
            Assert.Equal("UNOC", interchange.SyntaxIdentifier);
            Assert.Equal(3, interchange.SyntaxVersion);
            Assert.Equal("1234567891123", interchange.SenderId);
            Assert.Equal("14", interchange.PartnerIDCodeQualifier);
            Assert.Equal("7080005059275", interchange.RecipientId);
            Assert.Equal("14", interchange.ParterIDCode);
            Assert.Equal("SPOTMARKED", interchange.RoutingAddress);
            Assert.Equal(new DateTime(2012, 10, 10, 11, 4, 0), interchange.DateOfPreparation);
            Assert.Equal("HBQ001", interchange.ControlRef);


            var quote = interchange.QuoteMessage;

            //Test Quote Message Header
            Assert.Equal("1", quote.MessageRef);
            Assert.Equal("QUOTES", quote.MessageType);
            Assert.Equal("D", quote.Version);
            Assert.Equal("96A", quote.ReleaseNumber);
            Assert.Equal("UN", quote.ControllingAgency);
            Assert.Equal("EDIEL2", quote.AssociationAssignedCode);
            Assert.Equal("S", quote.CommonAccessRef);

            Assert.Equal("310", quote.MessageName);
            Assert.Equal("2010101900026812", quote.DocumentNumber);
            Assert.Equal("9", quote.MessageFunction);
            Assert.Equal("AB", quote.ResponseType);

            Assert.NotNull(interchange.QuoteMessage.MessageDate.Code);
            Assert.NotNull(interchange.QuoteMessage.ProcessingStartDate.Code);
            Assert.NotNull(interchange.QuoteMessage.ProcessingEndDate.Code);


            Assert.Equal(new DateTime(2010, 10, 19, 11, 04, 00), quote.MessageDate.Date);
            Assert.Equal(new DateTime(2010, 10, 19, 23, 00, 00), quote.ProcessingStartDate.Date);
            Assert.Equal(new DateTime(2010, 10, 20, 23, 00, 00), quote.ProcessingEndDate.Date);

            Assert.Equal(1, quote.UTCOffset.Hours);

            Assert.Equal("2", quote.CurrencyQualifier);
            Assert.Equal("SEK", quote.ISOCurrency);


            Assert.Equal(2, quote.NAD.Count);
            Assert.Equal("FR", quote.NAD[0].PartyQualifier);
            Assert.Equal("1234567891123", quote.NAD[0].PartyId);
            Assert.Equal("9", quote.NAD[0].ResponsibleAgency);

            Assert.Equal("DO", quote.NAD[1].PartyQualifier);
            Assert.Equal("7080005059275", quote.NAD[1].PartyId);
            Assert.Equal("9", quote.NAD[1].ResponsibleAgency);

            Assert.Equal("105", quote.LocationQualifier);
            Assert.Equal("SE1", quote.LocationId);
            Assert.Equal("SM", quote.LocationResponsibleAgency);


        }

        [Fact(Skip = "work in progress")]
        public void X12_Grammar_Test() {
            var grammar = EdiGrammar.NewX12();

            Assert.True(false);
        }
    }
}
