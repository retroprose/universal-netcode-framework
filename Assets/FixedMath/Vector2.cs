namespace FixedMath
{

    public struct Vector2
    {
        public Scaler x;
        public Scaler y;

        public Vector2(Scaler x_, Scaler y_)
        {
            x = x_;
            y = y_;
        }

        public override string ToString()
        {
            return $"{x}, {y}";
        }

        // comparison operators
        static public bool operator ==(Vector2 lhs, Vector2 rhs)
        {
            return lhs.x == rhs.x && lhs.y == rhs.y;
        }

        static public bool operator !=(Vector2 lhs, Vector2 rhs)
        {
            return lhs.x != rhs.x || lhs.y != rhs.y;
        }

        // friend functions
        static public Vector2 operator*(Vector2 v, Scaler s)
        {
            return new Vector2(v.x * s, v.y * s);
        }

        static public Vector2 operator/(Vector2 v, Scaler s)
        {
            return new Vector2(v.x / s, v.y / s);
        }

        static public Vector2 operator *(Scaler s, Vector2 v)
        {
            return new Vector2(s * v.x, s * v.y);
        }

        static public Vector2 operator /(Scaler s, Vector2 v)
        {
            return new Vector2(s / v.x, s / v.y);
        }

        // operations
        static public Vector2 operator -(Vector2 v)
	    {
            return new Vector2(-v.x, -v.y);
        }

        static public Vector2 operator +(Vector2 lhs, Vector2 rhs)
        {
            return new Vector2(lhs.x + rhs.x, lhs.y + rhs.y);
        }

        static public Vector2 operator -(Vector2 lhs, Vector2 rhs)
        {
            return new Vector2(lhs.x - rhs.x, lhs.y - rhs.y);
        }

		// left and right hand perpendicular vectors	
		public Vector2 Left()
		{
            return new Vector2(-y, x);
        }

		public Vector2 Right()
		{
            return new Vector2(y, -x);
        }

        // Dot products
        public Scaler Dot(ref Vector2 v)
        {
            return x * v.x + y * v.y;
        }

        public Scaler LeftDot(ref Vector2 v)
        {
            return y * v.x - x * v.y; ;
        }

        public Scaler RightDot(ref Vector2 v)
        {
            return x * v.y - y * v.x;
        }

        // vector length
        public Scaler Length()
		{
            return Scaler.Sqrt(x * x + y * y);
        }

        public Scaler LengthSquared()
		{
            return x * x + y * y;
        }

		// vector normlaization
		public Vector2 NormalizedCopy()
        {
			Scaler length = Scaler.Sqrt(x * x + y * y);
            if (length < Scaler.Epsilon) {
				return new Vector2(x, y);
			}
			Scaler invLength = Scaler.One / length;
    		return new Vector2(x * invLength, y * invLength);
		}

        public Vector2 NormalizedCopy(Scaler length)
        {
            if (length < Scaler.Epsilon)
            {
                return new Vector2(x, y);
            }
            Scaler invLength = Scaler.One / length;
            return new Vector2(x * invLength, y * invLength);
        }

		public Scaler Normalize()
        {
            Scaler length = Scaler.Sqrt(x * x + y * y);
            if (length < Scaler.Epsilon)
            {
                return Scaler.Zero;
            }
            Scaler invLength = Scaler.One / length;
            x *= invLength;
            y *= invLength;
            return length;
        }

        public Scaler Normalize(Scaler length)
        {
            if (length < Scaler.Epsilon)
            {
                return Scaler.Zero;
            }
            Scaler invLength = Scaler.One / length;
            x *= invLength;
            y *= invLength;
            return length;
        }

        /*Vector2 InvSqNormalizedCopy() const {
            Real invLength = InvSqRoot(x*x + y * y);
            Vector2 v(*this);
            v.x *= invLength;
            v.y *= invLength;
            return v;
        }

        void InvSqNormalize() {
            Real invLength = InvSqRoot(x*x + y * y);
            x *= invLength;
            y *= invLength;
        }*/

        public Vector2 MidPoint(ref Vector2 v)
        {
            return new Vector2((x + v.x) * (Scaler.One / 2), (y + v.y) * (Scaler.One / (Scaler)2));
        }

		// angle with respect to unit x axis
		public Scaler Angle()
		{
            return Scaler.Atan2(y, x);
        }

		// angle with respect to abritraty second vector
		public Scaler Angle(ref Vector2 v)
		{
            return Scaler.Atan2(y* v.x - x* v.y, x* v.x + y* v.y);
        }
	
		// rotate vector by angle
		public Vector2 Rotate(Scaler a)
        {
            Scaler c = Scaler.Cos(a);
            Scaler s = Scaler.Sin(a);
			return new Vector2(x* c - y* s, x* s + y* c);
		}
	
		public Vector2 Reflect(ref Vector2 n)
        {
            // -2 * (V dot N)*N + V
            return -((Scaler)2) * Dot(ref n) * n + this;
		}


    }

}