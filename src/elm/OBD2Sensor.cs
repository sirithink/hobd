﻿using System;
using System.Collections.Generic;

namespace hobd
{

public class OBD2Sensor : CoreSensor
{
    public Func<OBD2Sensor, double> obdValue;
    protected byte[] dataraw;
    protected int data_offset;
    
    public OBD2Sensor()
    {
        Command = -1;
    }

    public int Command { get; set; }
        
    string rawcommand;
    public virtual string RawCommand {
        get{
            if (rawcommand == null)
                return "01" + this.Command.ToString("X2");
            else
                return rawcommand;
        }
        set{
            rawcommand = value;
        }
    }

    public static byte to_h(byte a)
    {
        if (a >= 0x30 && a <= 0x39) return (byte)(a-0x30);
        if (a >= 0x41 && a <= 0x46) return (byte)(a+10-0x41);
        if (a >= 0x61 && a <= 0x66) return (byte)(a+10-0x61);
        return 255;
    }

    public virtual bool SetRawValue(byte[] msg)
    {
        var msgraw = new List<byte>();
        
        // parse reply
        for(int i = 0; i < msg.Length; i++)
        {
            var a = msg[i];
            if (a == ' ' || a == '\r' || a == '\n')
                continue;
            if (i+1 >= msg.Length)
                break;
            i++;
            var b = msg[i];
            a = to_h(a);
            b = to_h(b);
            if (a > 0x10 || b > 0x10)
                continue;
            
            msgraw.Add((byte)((a<<4) + b));
            
        }
        
        byte[] dataraw = msgraw.ToArray();

        return this.SetValue(dataraw);
    }

    public virtual bool SetValue(byte[] dataraw)
    {
        data_offset = 0;
        while(data_offset < dataraw.Length-1 && !(dataraw[data_offset] == 0x41 && dataraw[data_offset+1] == this.Command))
        {
            data_offset++;
        }
        if (data_offset >= dataraw.Length-1)
        {
            data_offset = 0;
            /*
               If we set OBD2 command explicitly,
               this means we should find this command in response.
               In case command was not found - response is assumed to be invalid.
            */
            if (Command != -1)
                return false;
        }

        this.dataraw = dataraw;

        try{
            this.Value = obdValue(this);
        }catch(Exception){
            string r = "";
            for (int i = 0; i < dataraw.Length; i++)
               r += dataraw[i].ToString("X2") + " ";
            Logger.error("OBD2Sensor", "Fail parsing sensor value: " + this.ID + " " + r);
            return false;
        }
        this.TimeStamp = DateTimeMs.Now;
        registry.TriggerListeners(this);
        return true;
    }
    
    public int len()
    {
        return dataraw.Length - (data_offset+2);
    }

    public double getraw(int idx)
    {
        return dataraw[idx];
    }
    public double getraw_word(int idx)
    {
        return (dataraw[idx]<<8) + dataraw[idx+1];
    }

    public double getraw_wordle(int idx)
    {
        return (dataraw[idx]) + (dataraw[idx+1]<<8);
    }

    public double getraw_dword(int idx)
    {
        return (dataraw[idx+0]<<24) + (dataraw[idx+1]<<16) + (dataraw[idx+2]<<8) + (dataraw[idx+3]<<0);
    }

    public double getraw_dwordle(int idx)
    {
        return (dataraw[idx+0]<<0) + (dataraw[idx+1]<<8) + (dataraw[idx+2]<<16) + (dataraw[idx+3]<<24);
    }

    public double get(int idx)
    {
        return getraw(data_offset+2+idx);
    }

    public double get_word(int idx)
    {
        return getraw_word(data_offset+2+idx);
    }

    public double get_bit(int idx, int bit)
    {
        return (dataraw[data_offset+2+idx] & (1<<bit)) != 0 ? 1 : 0;
    }

    public double getab()
    {
        return getraw_word(data_offset+2+0);
    }
    public double getbc()
    {
        return getraw_word(data_offset+2+1);
    }
    public double getcd()
    {
        return getraw_word(data_offset+2+2);
    }
    public double getde()
    {
        return getraw_word(data_offset+2+3);
    }
    
}

}
