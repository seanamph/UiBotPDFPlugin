using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

//建议把下面的namespace名字改为您的插件名字
namespace UiBotPDFPlugin
{
 public interface PDFPlugin_Interface
 {   //定义一个插件函数时，必须先在这个interface里面声明
  string TextFromPage(string _filePath, int startPage, int? endPage);
 }

 public class PDFPlugin_Implement : PDFPlugin_Interface
 {   //在这里实现插件函数
  public string TextFromPage(string _filePath, int startPage, int? endPage)
  {
   var pdfReader = new PdfReader(_filePath);


   var locationTextExtractionStrategy = new TextWithFontExtractionStategy( );
   string textFromPage = "";
   for (int i = startPage; i <= (endPage != null ? endPage: startPage); i++)
    textFromPage += PdfTextExtractor.GetTextFromPage(pdfReader, i, locationTextExtractionStrategy);

   return Encoding.UTF8.GetString(Encoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(textFromPage)));


  }
 }


 public class TextWithFontExtractionStategy : iTextSharp.text.pdf.parser.ITextExtractionStrategy
 {
  //HTML buffer
  private StringBuilder result = new StringBuilder();
  // 用來存放文字的矩形
  List<System.util.RectangleJ> rectText = new List<System.util.RectangleJ>();

  // 用來存放文字
  List<String> textList = new List<String>();

  // 用來存放文字的Y座標
  List<float> listY = new List<float>();
  List<float> listX = new List<float>();

  // 用來存放每一行文字的座標位置
  List<Dictionary<String, System.util.RectangleJ>> row_text_rect = new List<Dictionary<String, System.util.RectangleJ>>();

  Dictionary<String, String> text_position = new Dictionary<String, String>();
  // 圖片座標
  List<float[]> arrays = new List<float[]>();

  // 圖片
  List<byte[]> arraysByte = new List<byte[]>();

  //http://api.itextpdf.com/itext/com/itextpdf/text/pdf/parser/TextRenderInfo.html
  private enum TextRenderMode
  {
   FillText = 0,
   StrokeText = 1,
   FillThenStrokeText = 2,
   Invisible = 3,
   FillTextAndAddToPathForClipping = 4,
   StrokeTextAndAddToPathForClipping = 5,
   FillThenStrokeTextAndAddToPathForClipping = 6,
   AddTextToPaddForClipping = 7
  }



  public void RenderText(iTextSharp.text.pdf.parser.TextRenderInfo renderInfo)
  {
   String text = renderInfo.GetText().Trim();
  
   if (text.Length > 0)
   {
    System.util.RectangleJ rectBase = renderInfo.GetBaseline().GetBoundingRectange();
    // 獲取文字下面的矩形
    System.util.RectangleJ rectAscen = renderInfo.GetAscentLine().GetBoundingRectange();
    // 計算出文字的邊框矩形
    //float leftX = (float)rectBase.X;
    //float leftY = (float)(rectBase.Y - 1);
    //float rightX = (float)rectBase.Width;
    //float rightY = (float)(rectBase.Height - 1);
    //Rectangle r = rectBase.GetBounds();
    //// System.out.println("float:" + leftX + ":" + leftY + ":" + rightX
    //// + ":" + rightY);
    //Rectangle rect = new Rectangle(rectBase.X, leftY, rightX - leftX, rightY - leftY);
    //// System.out.println("text:" + text + "X:" + rect.x + "Y:" + rect.y
    //// + "width:" + rect.width + "height:"
    //// + rect.height);
    if (listY.Contains(rectBase.Y))
    {
     int index = listY.IndexOf(rectBase.Y);
     float tempx = rectBase.X > rectText[index].X ? rectText[index].X : rectBase.X;
     rectText[index] = new System.util.RectangleJ(tempx, rectBase.Y, rectBase.Width + rectText[index].Width, rectBase.Height);
     textList[index] = textList[index] + text;
    }
    else
    {
     rectText.Add(rectBase);
     textList.Add(text);
     listY.Add(rectBase.Y);
    }
    if (!listX.Contains(rectBase.X))
    {
     listX.Add(rectBase.X);
    }
    text_position[rectBase.X + "," + rectBase.Y] = text;


    Dictionary<String, System.util.RectangleJ> map = new Dictionary<String, System.util.RectangleJ>();
    map[text] = rectBase;
    row_text_rect.Add(map);
   }
  }

  public string GetResultantText()
  {
   string result = "";
   listY.Sort();
   listX.Sort();
   for (int j = listY.Count - 1; j >= 0; j--)
   {
    string line = "";
    bool first = true;
    float lastLeft = 0;
    for (int i = 0; i < listX.Count; i++)
     {
     if (text_position.ContainsKey(listX[i] + "," + listY[j]))
     {
      if (first)
      {
       for (int k = 0; k <= listX[i]; k += 20) line += " ";
      }
      else
      {
       for (float k = lastLeft; k <= listX[i] - 20; k += 20) line += " ";
      }
      line += text_position[listX[i] + "," + listY[j]];
      first = false;
      lastLeft = listX[i];
     }
    }
    if(line != "") result += line + "\n";
   }
   return result;
  }

  //Not needed
  public void BeginTextBlock() { }
  public void EndTextBlock() { }
  public void RenderImage(ImageRenderInfo renderInfo) { }
 }
}
