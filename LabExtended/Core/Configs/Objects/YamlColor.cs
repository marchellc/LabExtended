using System.ComponentModel;

using UnityEngine;

using YamlDotNet.Serialization;

namespace LabExtended.Core.Configs.Objects
{
    /// <summary>
    /// Represents a YAML-serializable <see cref="UnityEngine.Color"/> alternative.
    /// </summary>
    public class YamlColor
    {
        private Color? cachedColor;

        /// <summary>
        /// Gets or sets the value of the color's alpha.
        /// </summary>
        [Description("The alpha component of the color.")]
        public float A { get; set; }

        /// <summary>
        /// Gets or sets the red component of the color.
        /// </summary>
        [Description("The red component of the color.")]
        public float R { get; set; }

        /// <summary>
        /// Gets or sets the green component of the color.
        /// </summary>
        [Description("The green component of the color.")]
        public float G { get; set; }

        /// <summary>
        /// Gets or sets the blue component of the color.
        /// </summary>
        [Description("The blue component of the color.")]
        public float B { get; set; }

        /// <summary>
        /// Gets the converted Unity <see cref="Color"/>.
        /// </summary>
        [YamlIgnore]
        public Color Color
        {
            get
            {
                if (!cachedColor.HasValue 
                    || cachedColor.Value.a != A
                    || cachedColor.Value.r != R
                    || cachedColor.Value.g != G
                    || cachedColor.Value.b != B)
                    cachedColor = new Color(R, G, B, A);

                return cachedColor.Value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="YamlColor"/> class.
        /// </summary>
        public YamlColor()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="YamlColor"/> class using the specified <see cref="Color"/>.
        /// </summary>
        /// <param name="color">The <see cref="Color"/> instance from which to initialize the <see cref="YamlColor"/>. The alpha, red,
        /// green, and blue components of the color are copied to the corresponding properties of the <see
        /// cref="YamlColor"/>.</param>
        public YamlColor(Color color)
        {
            A = color.a;
            R = color.r;
            G = color.g;
            B = color.b;

            cachedColor = color;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="YamlColor"/> struct with the specified color components.
        /// </summary>
        /// <param name="a">The alpha component of the color, representing transparency.</param>
        /// <param name="r">The red component of the color.</param>
        /// <param name="g">The green component of the color.</param>
        /// <param name="b">The blue component of the color.</param>
        public YamlColor(float a, float r, float g, float b)
        {
            A = a;
            R = r;
            G = g;
            B = b;

            cachedColor = new Color(r, g, b, a);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="YamlColor"/> to <see cref="Color"/>.
        /// </summary>
        /// <param name="yamlColor">The <see cref="YamlColor"/> instance to convert. Cannot be <see langword="null"/>.</param>
        public static implicit operator Color(YamlColor yamlColor) 
            => yamlColor?.Color ?? throw new ArgumentNullException(nameof(yamlColor));
    }
}