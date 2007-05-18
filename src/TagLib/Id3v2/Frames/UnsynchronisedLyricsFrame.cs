using System;
using System.Collections.Generic;
using System.Text;
using TagLib.Id3v2;

namespace TagLib.Id3v2
{
    public class UnsynchronisedLyricsFrame : Frame
    {
      #region Private Properties
      private StringType encoding      = StringType.UTF8;
      private ByteVector language      = null;
      private string     description   = null;
      private string     text          = null;
      #endregion
      
      
      
      #region Constructors
      public UnsynchronisedLyricsFrame (string description, ByteVector language, StringType encoding) : base ("USLT", 4)
      {
         this.encoding    = encoding;
         this.language    = language;
         this.description = description;
      }
      
      public UnsynchronisedLyricsFrame (string description, ByteVector language) : this (description, language, TagLib.Id3v2.Tag.DefaultEncoding)
      {}

      public UnsynchronisedLyricsFrame (string description) : this (description, null)
      {}
      
      public UnsynchronisedLyricsFrame(ByteVector data, uint version) : base(data, version)
      {
         SetData (data, 0, version);
      }
      
      protected internal UnsynchronisedLyricsFrame(ByteVector data, int offset, FrameHeader h, uint version) : base(h)
      {
         ParseFields (FieldData (data, offset, version), version);
      }
      #endregion
      
      
      
      #region Public Properties
      public StringType TextEncoding
      {
         get {return encoding;}
         set {encoding = value;}
      }

      public ByteVector Language
      {
         get {return language != null ? language : "XXX";}
         set {language = value != null ? value.Mid (0, 3) : "XXX";}
      }
      
      public string Description
      {
         get {return description;}
         set {description = value;}
      }
      
      public string Text
      {
         get {return text;}
         set {text = value;}
      }
      #endregion
      
      
      
      #region Public Methods
      public override string ToString ()
      {
         return text;
      }
      
      public override void SetText (string text)
      {
         this.text = text;
      }
      #endregion
      
      
      
      #region Public Static Methods
      public static UnsynchronisedLyricsFrame Get (Tag tag, string description, ByteVector language, bool create)
      {
         foreach (Frame f in tag.GetFrames ("USLT"))
         {
            UnsynchronisedLyricsFrame cf = f as UnsynchronisedLyricsFrame;
            
            if (cf != null && cf.Description == description && (language == null || language == cf.Language))
               return cf;
         }
         
         if (!create)
            return null;
         
         UnsynchronisedLyricsFrame frame = new UnsynchronisedLyricsFrame (description, language);
         tag.AddFrame (frame);
         return frame;
      }
      
      public static UnsynchronisedLyricsFrame GetPreferred (Tag tag, string description, ByteVector language)
      {
         // This is weird, so bear with me. The best thing we can have is 
         // something straightforward and in our own language. If it has a 
         // description, then it is probably used for something other than
         // an actual comment. If that doesn't work, we'd still rather have 
         // something in our language than something in another. After that
         // all we have left are things in other languages, so we'd rather 
         // have one with actual content, so we try to get one with no 
         // description first.
         
         int best_value = -1;
         UnsynchronisedLyricsFrame best_frame = null;
         
         foreach (Frame f in tag.GetFrames ("USLT"))
         {
            UnsynchronisedLyricsFrame cf = f as UnsynchronisedLyricsFrame;
            if (cf == null) continue;
            
            bool same_name = cf.Description == description;
            bool same_lang = cf.Language == language;
            
            if (same_name && same_lang) return cf;
            
            int value = same_lang ? 2 : same_name ? 1 : 0;
            
            if (value <= best_value)
               continue;
            
            best_value = value;
            best_frame = cf;
         }
         
         return best_frame;
      }
      #endregion
      
      
      
      
      #region Protected Methods
      protected override void ParseFields (ByteVector data, uint version)
      {
         if (data.Count < 4)
            throw new CorruptFileException ("Not enough bytes in field.");
         
         encoding = (StringType) data [0];
         language = data.Mid (1, 3);

         string [] split = data.ToStrings (encoding, 4, 2);
         
         if (split.Length == 1)
         {
            // Bad comment frame. Assume that it lacks a description.
            description = String.Empty;
            text        = split [0];
         }
         else
         {
            description = split [0];
            text        = split [1];
         }
      }
      
      protected override ByteVector RenderFields(uint version)
      {
         StringType encoding = CorrectEncoding(TextEncoding, version);
         ByteVector v = new ByteVector();

         v.Add((byte)encoding);
         v.Add(Language);
         v.Add(ByteVector.FromString (description, encoding));
         v.Add(TextDelimiter(encoding));
         v.Add(ByteVector.FromString (text, encoding));

         return v;
      }
      #endregion
   }
}