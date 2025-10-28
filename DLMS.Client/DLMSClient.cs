﻿//
// --------------------------------------------------------------------------
//  Gurux Ltd
//
//
//
// Filename:        $HeadURL$
//
// Version:         $Revision$,
//                  $Date$
//                  $Author$
//
// Copyright (c) Gurux Ltd
//
//---------------------------------------------------------------------------
//
//  DESCRIPTION
//
// This file is a part of Gurux Device Framework.
//
// Gurux Device Framework is Open Source software; you can redistribute it
// and/or modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; version 2 of the License.
// Gurux Device Framework is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU General Public License for more details.
//
// More information of Gurux products: http://www.gurux.org
//
// This code is licensed under the GNU General Public License v2.
// Full text may be retrieved at http://www.gnu.org/licenses/gpl-2.0.txt
//---------------------------------------------------------------------------
using DLMS.Client.GXMedia.Mqtt;
using Gurux.DLMS.Enums;
using Gurux.DLMS.Objects;
using Gurux.Net;
using Gurux.Serial;

namespace DLMS.Client;

public class DLMSClient : IDisposable
{
    private bool isAssociationViewReaded = false;
    private bool isInitialized = false;
    private GXDLMSReader reader;
    private Settings settings;
    public DLMSClient(string[] args, Settings settings)
    {
        this.settings = settings;
        ////////////////////////////////////////
        //Handle command line parameters.
        int ret = Settings.GetParameters(args, settings);
        if (ret != 0) throw new ArgumentNullException("Argument can't be null.");
        settings.client.OnPdu += (sender, data) =>
        {
            try
            {
                /*
                //Encrypted PDUs are converted to XML.
                GXDLMSTranslator translator = new GXDLMSTranslator();
                translator.Comments = true;
                translator.SecuritySuite = settings.client.Ciphering.SecuritySuite;
                translator.BlockCipherKey = settings.client.Ciphering.BlockCipherKey;
                translator.AuthenticationKey = settings.client.Ciphering.AuthenticationKey;
                string xml = translator.PduToXml(data);
                Console.WriteLine(xml);
                */
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        };
        ////////////////////////////////////////
        //Initialize connection settings.
        if (settings.media is GXSerial) { }
        else if (settings.media is GXNet) { }
        else if (settings.media is GXMqtt mqtt) { }
        else throw new Exception("Unknown media type.");
        ////////////////////////////////////////
        reader = new GXDLMSReader(settings.client,
            settings.media, settings.trace,
            settings.invocationCounter, settings.WaitTime);
        reader.OnNotification += (data) =>
        {
            Console.WriteLine(data);
        };
        //Create manufacturer spesific custom COSEM object.
        settings.client.OnCustomObject += (type, version) =>
        {
            /*
            if (type == 6001 && version == 0)
            {
                return new ManufacturerSpesificObject();
            }
            */
            return null;
        };

        try { settings.media.Open(); }
        catch (System.IO.IOException ex)
        {
            Console.WriteLine("----------------------------------------------------------");
            Console.WriteLine(ex.Message);
            Console.WriteLine("Available ports:");
            Console.WriteLine(string.Join(" ", GXSerial.GetPortNames()));
            return;
        }
        //Some meters need a break here.
        Thread.Sleep(1000);
        Console.WriteLine("Connected:");

        if (settings.media is GXNet net && settings.client.InterfaceType == InterfaceType.CoAP)
        {
            //Update token ID.
            settings.client.Coap.Token = 0x45;
            settings.client.Coap.Host = net.HostName;
            settings.client.Coap.MessageId = 1;
            settings.client.Coap.Port = (UInt16)net.Port;
            //DLMS version.
            settings.client.Coap.Options[65001] = (byte)1;
            //Client SAP.
            settings.client.Coap.Options[65003] = (byte)settings.client.ClientAddress;
            //Server SAP
            settings.client.Coap.Options[65005] = (byte)settings.client.ServerAddress;
        }
        //set cache associationView
        if (settings.outputFile != null)
        {
            try
            {
                settings.client.Objects.Clear();
                settings.client.Objects.AddRange(GXDLMSObjectCollection.Load(settings.outputFile));
                isAssociationViewReaded = true;
            }
            catch (Exception)
            {
                //It's OK if this fails.
            }
        }
    }

    public void Dispose()
    {
        reader.Close();
    }

    public void ExportMeterCertificate()
    {
        //Export client and server certificates from the meter.
        if (!string.IsNullOrEmpty(settings.ExportSecuritySetupLN))
        {
            reader.ExportMeterCertificates(settings.ExportSecuritySetupLN);
        }
    }
    public void GenerateCertificate()
    {
        //Generate new client and server certificates and import them to the server.
        if (!string.IsNullOrEmpty(settings.GenerateSecuritySetupLN))
        {
            reader.GenerateCertificates(settings.GenerateSecuritySetupLN);
        }

    }
    public void ReadObject(List<KeyValuePair<string, int>> readObjects)
    {
        if (!isInitialized)
        {
            reader.InitializeConnection();
            if (!isAssociationViewReaded) reader.GetAssociationView(settings.outputFile);
        }

        //if (settings.readObjects.Count != 0)
        if (readObjects.Count != 0)
        {
            foreach (KeyValuePair<string, int> it in readObjects)
            {
                object val = reader.Read(settings.client.Objects.FindByLN(ObjectType.None, it.Key), it.Value);
                reader.ShowValue(val, it.Value);
            }
            if (settings.outputFile != null)
            {
                try
                {
                    settings.client.Objects.Save(settings.outputFile, new GXXmlWriterSettings() { UseMeterTime = true, IgnoreDefaultValues = false });
                }
                catch (Exception)
                {
                    //It's OK if this fails.
                }
            }
        }
    }
}
