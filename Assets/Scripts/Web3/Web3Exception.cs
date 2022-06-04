using System;

class Web3Exception : Exception
{
    public int Code { get; }

    public Web3Exception(string message, int code) : base(message)
    {
        Code = code;
    }

    public Web3Exception(string message, int code, Exception innerException) : base(message, innerException)
    {
        Code = code;
    }
}
