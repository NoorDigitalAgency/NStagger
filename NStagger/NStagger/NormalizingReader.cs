using System;
using System.IO;
using System.Text;

namespace NStagger
{
    public class NormalizingReader:StreamReader
    {
        private readonly NormalizationForm form;

        public NormalizingReader(Stream stream, bool canonical) : base(stream)
        {
            form = canonical ? NormalizationForm.FormKC : NormalizationForm.FormC;
        }

        public NormalizingReader(Stream stream, bool detectEncodingFromByteOrderMarks, bool canonical) : base(stream, detectEncodingFromByteOrderMarks)
        {
            form = canonical ? NormalizationForm.FormKC : NormalizationForm.FormC;
        }

        public NormalizingReader(Stream stream, Encoding encoding, bool canonical) : base(stream, encoding)
        {
            form = canonical ? NormalizationForm.FormKC : NormalizationForm.FormC;
        }

        public NormalizingReader(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks, bool canonical) : base(stream, encoding, detectEncodingFromByteOrderMarks)
        {
            form = canonical ? NormalizationForm.FormKC : NormalizationForm.FormC;
        }

        public NormalizingReader(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize, bool canonical) : base(stream, encoding, detectEncodingFromByteOrderMarks, bufferSize)
        {
            form = canonical ? NormalizationForm.FormKC : NormalizationForm.FormC;
        }

        public NormalizingReader(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize, bool leaveOpen, bool canonical) : base(stream, encoding, detectEncodingFromByteOrderMarks, bufferSize, leaveOpen)
        {
            form = canonical ? NormalizationForm.FormKC : NormalizationForm.FormC;
        }

        public NormalizingReader(string path, bool canonical) : base(path)
        {
            form = canonical ? NormalizationForm.FormKC : NormalizationForm.FormC;
        }

        public NormalizingReader(string path, bool detectEncodingFromByteOrderMarks, bool canonical) : base(path, detectEncodingFromByteOrderMarks)
        {
            form = canonical ? NormalizationForm.FormKC : NormalizationForm.FormC;
        }

        public NormalizingReader(string path, Encoding encoding, bool canonical) : base(path, encoding)
        {
            form = canonical ? NormalizationForm.FormKC : NormalizationForm.FormC;
        }

        public NormalizingReader(string path, Encoding encoding, bool detectEncodingFromByteOrderMarks, bool canonical) : base(path, encoding, detectEncodingFromByteOrderMarks)
        {
            form = canonical ? NormalizationForm.FormKC : NormalizationForm.FormC;
        }

        public NormalizingReader(string path, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize, bool canonical) : base(path, encoding, detectEncodingFromByteOrderMarks, bufferSize)
        {
            form = canonical ? NormalizationForm.FormKC : NormalizationForm.FormC;
        }

        public override int Read(char[] buffer, int index, int count)
        {
            int read = base.Read(buffer, index, count);

            if (read <= 0)
            {
                return read;
            }

            string text = new string(buffer, index, read);

            string normalized = text.Normalize(form);

            char[] normalizedChars = normalized.ToCharArray();

            Array.Copy(normalizedChars, 0, buffer, index, count);

            return normalized.Length;
        }
    }
}