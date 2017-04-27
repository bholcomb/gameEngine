using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.ComponentModel;
using OpenTK.Graphics;

namespace OpenTK
{
   public class Color4TypeConverter : ExpandableObjectConverter
   {

      public override bool CanConvertTo(System.ComponentModel.ITypeDescriptorContext context, System.Type destinationType)
      {
         if (object.ReferenceEquals(destinationType, typeof(Color4)))
         {
            return true;
         }

         return base.CanConvertTo(context, destinationType);
      }
      public override object ConvertTo(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, System.Type destinationType)
      {
         if (object.ReferenceEquals(destinationType, typeof(string)) && object.ReferenceEquals(value.GetType(), typeof(Color4)))
         {
            return value.ToString();
         }

         return base.ConvertTo(context, culture, value, destinationType);
      }

      public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Type sourceType)
      {
         if (object.ReferenceEquals(sourceType, typeof(string)))
         {
            return true;
         }

         return base.CanConvertFrom(context, sourceType);
      }

      public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
      {
         if (value is string)
         {
            string Text = Convert.ToString(value);

            if (Text.Contains("="))
            {
               int Start = Text.IndexOf('(', Text.IndexOf('='));
               Text = Text.Substring(Start + 1);
               Text = Text.Substring(0, Text.IndexOf(")"));
               string[] TextSplit = Text.Split(new string[] { ", " }, StringSplitOptions.None);
               return new Color4(float.Parse(TextSplit[0]), float.Parse(TextSplit[1]), float.Parse(TextSplit[2]), float.Parse(TextSplit[3]));
            }
            else
            {
               Text = Text.Trim('(').Trim(')');
               string[] TextSplit = Text.Split(new string[] { ", " }, StringSplitOptions.None);
               return new Color4(float.Parse(TextSplit[0]), float.Parse(TextSplit[1]), float.Parse(TextSplit[2]), float.Parse(TextSplit[3]));
            }

         }
         return base.ConvertFrom(context, culture, value);
      }

      public override object CreateInstance(System.ComponentModel.ITypeDescriptorContext context, System.Collections.IDictionary propertyValues)
      {
         return new Color4(Convert.ToSingle(propertyValues["R"]), Convert.ToSingle(propertyValues["G"]), Convert.ToSingle(propertyValues["B"]), Convert.ToSingle(propertyValues["A"]));
      }

      public override System.ComponentModel.PropertyDescriptorCollection GetProperties(System.ComponentModel.ITypeDescriptorContext Context, object Value, System.Attribute[] Attributes)
      {
         List<PropertyDescriptor> Properties = new List<PropertyDescriptor>();
         Type ValueType = Value.GetType();
         FieldInfo[] ValueFields = ValueType.GetFields();

         foreach (FieldInfo Field in ValueFields)
         {
            if (Field.Name == "R" | Field.Name == "G" | Field.Name == "B" | Field.Name == "A")
            {
               FieldPropertyDescriptor Descriptor = new FieldPropertyDescriptor(typeof(Color4), Field, 0f);
               Properties.Add(Descriptor);
            }
         }
         PropertyDescriptorCollection Collection = new PropertyDescriptorCollection(Properties.ToArray());
         return Collection.Sort(new string[] {
                       "R",
                      "G",
                      "B",
                       "A"
               });
      }
      public override bool GetPropertiesSupported(System.ComponentModel.ITypeDescriptorContext context)
      {
         return true;
      }
   }

   public class Vector4TypeConverter : ExpandableObjectConverter
   {

      public override bool CanConvertTo(System.ComponentModel.ITypeDescriptorContext context, System.Type destinationType)
      {
         if (object.ReferenceEquals(destinationType, typeof(Vector4)))
         {
            return true;
         }

         return base.CanConvertTo(context, destinationType);
      }
      public override object ConvertTo(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, System.Type destinationType)
      {
         if (object.ReferenceEquals(destinationType, typeof(string)) && object.ReferenceEquals(value.GetType(), typeof(Vector4)))
         {
            return value.ToString();
         }

         return base.ConvertTo(context, culture, value, destinationType);
      }

      public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Type sourceType)
      {
         if (object.ReferenceEquals(sourceType, typeof(string)))
         {
            return true;
         }

         return base.CanConvertFrom(context, sourceType);
      }

      public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
      {
         if (value is string)
         {
            string Text = Convert.ToString(value).Trim().Trim('(').Trim(')');
            string[] TextSplit = Text.Split(new string[] { ", " }, StringSplitOptions.None);
            return new Vector4(float.Parse(TextSplit[0]), float.Parse(TextSplit[1]), float.Parse(TextSplit[2]), float.Parse(TextSplit[3]));
         }
         return base.ConvertFrom(context, culture, value);
      }

      public override object CreateInstance(System.ComponentModel.ITypeDescriptorContext context, System.Collections.IDictionary propertyValues)
      {
         return new Vector4(Convert.ToSingle(propertyValues["X"]), Convert.ToSingle(propertyValues["Y"]), Convert.ToSingle(propertyValues["Z"]), Convert.ToSingle(propertyValues["W"]));
      }

      public override System.ComponentModel.PropertyDescriptorCollection GetProperties(System.ComponentModel.ITypeDescriptorContext Context, object Value, System.Attribute[] Attributes)
      {
         List<PropertyDescriptor> Properties = new List<PropertyDescriptor>();
         Type ValueType = Value.GetType();
         FieldInfo[] ValueFields = ValueType.GetFields();

         foreach (FieldInfo Field in ValueFields)
         {
            if (Field.Name == "X" | Field.Name == "Y" | Field.Name == "Z" | Field.Name == "W")
            {
               FieldPropertyDescriptor Descriptor = new FieldPropertyDescriptor(typeof(Vector4), Field, 0f);
               Properties.Add(Descriptor);
            }
         }

         return new PropertyDescriptorCollection(Properties.ToArray());
      }
      public override bool GetPropertiesSupported(System.ComponentModel.ITypeDescriptorContext context)
      {
         return true;
      }
   }

   public class Vector3TypeConverter : ExpandableObjectConverter
   {
      public override bool CanConvertTo(System.ComponentModel.ITypeDescriptorContext context, System.Type destinationType)
      {
         if (object.ReferenceEquals(destinationType, typeof(Vector3)))
         {
            return true;
         }

         return base.CanConvertTo(context, destinationType);
      }
      public override object ConvertTo(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, System.Type destinationType)
      {
         if (object.ReferenceEquals(destinationType, typeof(string)) && object.ReferenceEquals(value.GetType(), typeof(Vector3)))
         {
            return value.ToString();
         }

         return base.ConvertTo(context, culture, value, destinationType);
      }

      public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Type sourceType)
      {
         if (object.ReferenceEquals(sourceType, typeof(string)))
         {
            return true;
         }

         return base.CanConvertFrom(context, sourceType);
      }

      public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
      {
         if (value is string)
         {
            string Text = Convert.ToString(value).Trim().Trim('(').Trim(')');
            string[] TextSplit = Text.Split(new string[] { ", " }, StringSplitOptions.None);
            return new Vector3(float.Parse(TextSplit[0]), float.Parse(TextSplit[1]), float.Parse(TextSplit[2]));
         }
         return base.ConvertFrom(context, culture, value);
      }

      public override object CreateInstance(System.ComponentModel.ITypeDescriptorContext context, System.Collections.IDictionary propertyValues)
      {
         return new Vector3(Convert.ToSingle(propertyValues["X"]), Convert.ToSingle(propertyValues["Y"]), Convert.ToSingle(propertyValues["Z"]));
      }

      public override System.ComponentModel.PropertyDescriptorCollection GetProperties(System.ComponentModel.ITypeDescriptorContext Context, object Value, System.Attribute[] Attributes)
      {
         List<PropertyDescriptor> Properties = new List<PropertyDescriptor>();
         Type ValueType = Value.GetType();
         FieldInfo[] ValueFields = ValueType.GetFields();

         foreach (FieldInfo Field in ValueFields)
         {
            if (Field.Name == "X" | Field.Name == "Y" | Field.Name == "Z")
            {
               FieldPropertyDescriptor Descriptor = new FieldPropertyDescriptor(typeof(Vector3), Field, 0f);
               Properties.Add(Descriptor);
            }
         }

         return new PropertyDescriptorCollection(Properties.ToArray());
      }
      public override bool GetPropertiesSupported(System.ComponentModel.ITypeDescriptorContext context)
      {
         return true;
      }
   }

   public class Vector2TypeConverter : ExpandableObjectConverter
   {

      public override bool CanConvertTo(System.ComponentModel.ITypeDescriptorContext context, System.Type destinationType)
      {
         if (object.ReferenceEquals(destinationType, typeof(Vector2)))
         {
            return true;
         }

         return base.CanConvertTo(context, destinationType);
      }
      public override object ConvertTo(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, System.Type destinationType)
      {
         if (object.ReferenceEquals(destinationType, typeof(string)) && object.ReferenceEquals(value.GetType(), typeof(Vector2)))
         {
            return value.ToString();
         }

         return base.ConvertTo(context, culture, value, destinationType);
      }

      public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Type sourceType)
      {
         if (object.ReferenceEquals(sourceType, typeof(string)))
         {
            return true;
         }

         return base.CanConvertFrom(context, sourceType);
      }

      public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
      {
         if (value is string)
         {
            string Text = Convert.ToString(value).Trim().Trim('(').Trim(')');
            string[] TextSplit = Text.Split(new string[] { ", " }, StringSplitOptions.None);
            return new Vector2(float.Parse(TextSplit[0]), float.Parse(TextSplit[1]));
         }
         return base.ConvertFrom(context, culture, value);
      }

      public override object CreateInstance(System.ComponentModel.ITypeDescriptorContext context, System.Collections.IDictionary propertyValues)
      {
         return new Vector2(Convert.ToSingle(propertyValues["X"]), Convert.ToSingle(propertyValues["Y"]));
      }

      public override System.ComponentModel.PropertyDescriptorCollection GetProperties(System.ComponentModel.ITypeDescriptorContext Context, object Value, System.Attribute[] Attributes)
      {
         List<PropertyDescriptor> Properties = new List<PropertyDescriptor>();
         Type ValueType = Value.GetType();
         FieldInfo[] ValueFields = ValueType.GetFields();

         foreach (FieldInfo Field in ValueFields)
         {
            if (Field.Name == "X" | Field.Name == "Y")
            {
               FieldPropertyDescriptor Descriptor = new FieldPropertyDescriptor(typeof(Vector2), Field, 0f);
               Properties.Add(Descriptor);
            }
         }

         return new PropertyDescriptorCollection(Properties.ToArray());
      }
      public override bool GetPropertiesSupported(System.ComponentModel.ITypeDescriptorContext context)
      {
         return true;
      }
   }

   public class FieldPropertyDescriptor : PropertyDescriptor
   {

      private FieldInfo MyFieldInfo;
      private Type MyCompontentType;

      private object MyDefaultValue;
      public FieldInfo FieldInfo
      {
         get { return this.MyFieldInfo; }
      }
      public object DefaultValue
      {
         get { return this.MyDefaultValue; }
      }

      public FieldPropertyDescriptor(Type ComponentType, FieldInfo FieldInfo)
         : base(FieldInfo.Name, new Attribute[] { new DescriptionAttribute("LOOOOOL") })
      {
         this.MyFieldInfo = FieldInfo;
         this.MyDefaultValue = null;
         this.MyCompontentType = ComponentType;
      }
      public FieldPropertyDescriptor(Type ComponentType, FieldInfo FieldInfo, object DefaultValue)
         : this(ComponentType, FieldInfo)
      {
         this.MyDefaultValue = DefaultValue;
      }

      public override bool CanResetValue(object component)
      {
         return true;
      }

      public override System.Type ComponentType
      {
         get { return this.MyCompontentType; }
      }

      public override object GetValue(object component)
      {
         return this.MyFieldInfo.GetValue(component);
      }

      public override bool IsReadOnly
      {
         get { return false; }
      }

      public override System.Type PropertyType
      {
         get { return this.MyFieldInfo.FieldType; }
      }

      public override void ResetValue(object component)
      {
         ValueType BoxedValue = (ValueType)component;
         this.MyFieldInfo.SetValue(BoxedValue, this.MyDefaultValue);
         component = BoxedValue;
      }

      public override void SetValue(object component, object value)
      {
         ValueType BoxedValue = (ValueType)component;
         this.MyFieldInfo.SetValue(BoxedValue, value);
         component = BoxedValue;
      }

      public override bool ShouldSerializeValue(object component)
      {
         return false;
      }
   }
}
