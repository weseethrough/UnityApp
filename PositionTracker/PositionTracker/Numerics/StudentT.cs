// <copyright file="StudentT.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2013 Math.NET
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
// </copyright>

using System;
using System.Collections.Generic;
//using MathNet.Numerics.Random;
//using MathNet.Numerics.RootFinding;

namespace MathNet.Numerics.Distributions
{
    public class Normal {
		
        /// <summary>
        /// Computes the cumulative distribution (CDF) of the distribution at x, i.e. P(X ≤ x).
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative distribution function.</param>
        /// <param name="mean">The mean (μ) of the normal distribution.</param>
        /// <param name="stddev">The standard deviation (σ) of the normal distribution. Range: σ ≥ 0.</param>
        /// <returns>the cumulative distribution at location <paramref name="x"/>.</returns>
        /// <seealso cref="CumulativeDistribution"/>
        /// <remarks>MATLAB: normcdf</remarks>
        public static double CDF(double mean, double stddev, double x)
        {
            if (stddev < 0.0) throw new ArgumentException("InvalidDistributionParameters");

            return 0.5*(1.0 + SpecialFunctions.Erf((x - mean)/(stddev*Constants.Sqrt2)));
        }

	}
	
	/// <summary>
    /// Continuous Univariate Student's T-distribution.
    /// Implements the univariate Student t-distribution. For details about this
    /// distribution, see
    /// <a href="http://en.wikipedia.org/wiki/Student%27s_t-distribution">
    /// Wikipedia - Student's t-distribution</a>.
    /// </summary>
    /// <remarks><para>We use a slightly generalized version (compared to
    /// Wikipedia) of the Student t-distribution. Namely, one which also
    /// parameterizes the location and scale. See the book "Bayesian Data
    /// Analysis" by Gelman et al. for more details.</para>
    /// <para>The density of the Student t-distribution  p(x|mu,scale,dof) =
    /// Gamma((dof+1)/2) (1 + (x - mu)^2 / (scale * scale * dof))^(-(dof+1)/2) /
    /// (Gamma(dof/2)*Sqrt(dof*pi*scale)).</para>
    /// <para>The distribution will use the <see cref="System.Random"/> by
    /// default.  Users can get/set the random number generator by using the
    /// <see cref="RandomSource"/> property.</para>
    /// <para>The statistics classes will check all the incoming parameters
    /// whether they are in the allowed range. This might involve heavy
    /// computation. Optionally, by setting Control.CheckDistributionParameters
    /// to <c>false</c>, all parameter checks can be turned off.</para></remarks>
    public class StudentT //: IContinuousDistribution
    {
        System.Random _random = new Random((int) DateTime.Now.Ticks & 0x0000FFFF);

        double _location;
        double _scale;
        double _freedom;

        /// <summary>
        /// Initializes a new instance of the StudentT class. This is a Student t-distribution with location 0.0
        /// scale 1.0 and degrees of freedom 1.
        /// </summary>
        public StudentT()
        {
            //_random = SystemRandomSource.Default;
            SetParameters(0.0, 1.0, 1.0);
        }

        /// <summary>
        /// Initializes a new instance of the StudentT class with a particular location, scale and degrees of
        /// freedom.
        /// </summary>
        /// <param name="location">The location (μ) of the distribution.</param>
        /// <param name="scale">The scale (σ) of the distribution. Range: σ > 0.</param>
        /// <param name="freedom">The degrees of freedom (ν) for the distribution. Range: ν > 0.</param>
        public StudentT(double location, double scale, double freedom)
        {
            //_random = SystemRandomSource.Default;
            SetParameters(location, scale, freedom);
        }

        /// <summary>
        /// A string representation of the distribution.
        /// </summary>
        /// <returns>a string representation of the distribution.</returns>
        public override string ToString()
        {
            return "StudentT(μ = " + _location + ", σ = " + _scale + ", ν = " + _freedom + ")";
        }

        /// <summary>
        /// Sets the parameters of the distribution after checking their validity.
        /// </summary>
        /// <param name="location">The location (μ) of the distribution.</param>
        /// <param name="scale">The scale (σ) of the distribution. Range: σ > 0.</param>
        /// <param name="freedom">The degrees of freedom (ν) for the distribution. Range: ν > 0.</param>
        /// <exception cref="ArgumentOutOfRangeException">When the parameters are out of range.</exception>
        void SetParameters(double location, double scale, double freedom)
        {
            if (scale <= 0.0 || freedom <= 0.0 || Double.IsNaN(scale) || Double.IsNaN(location) || Double.IsNaN(freedom))
            {
                throw new ArgumentException("InvalidDistributionParameters");
            }

            _location = location;
            _scale = scale;
            _freedom = freedom;
        }

        /// <summary>
        /// Gets or sets the location (μ) of the Student t-distribution.
        /// </summary>
        public double Location
        {
            get { return _location; }
            set { SetParameters(value, _scale, _freedom); }
        }

        /// <summary>
        /// Gets or sets the scale (σ) of the Student t-distribution. Range: σ > 0.
        /// </summary>
        public double Scale
        {
            get { return _scale; }
            set { SetParameters(_location, value, _freedom); }
        }

        /// <summary>
        /// Gets or sets the degrees of freedom (ν) of the Student t-distribution. Range: ν > 0.
        /// </summary>
        public double DegreesOfFreedom
        {
            get { return _freedom; }
            set { SetParameters(_location, _scale, value); }
        }

        /// <summary>
        /// Gets the mean of the Student t-distribution.
        /// </summary>
        public double Mean
        {
            get { return _freedom > 1.0 ? _location : Double.NaN; }
        }

        /// <summary>
        /// Gets the variance of the Student t-distribution.
        /// </summary>
        public double Variance
        {
            get
            {
                if (Double.IsPositiveInfinity(_freedom))
                {
                    return _scale*_scale;
                }

                if (_freedom > 2.0)
                {
                    return _freedom*_scale*_scale/(_freedom - 2.0);
                }

                return _freedom > 1.0 ? Double.PositiveInfinity : Double.NaN;
            }
        }

        /// <summary>
        /// Gets the standard deviation of the Student t-distribution.
        /// </summary>
        public double StdDev
        {
            get
            {
                if (Double.IsPositiveInfinity(_freedom))
                {
                    return Math.Sqrt(_scale*_scale);
                }

                if (_freedom > 2.0)
                {
                    return Math.Sqrt(_freedom*_scale*_scale/(_freedom - 2.0));
                }

                return _freedom > 1.0 ? Double.PositiveInfinity : Double.NaN;
            }
        }


        /// <summary>
        /// Gets the skewness of the Student t-distribution.
        /// </summary>
        public double Skewness
        {
            get
            {
                if (_freedom <= 3) throw new NotSupportedException();

                return 0.0;
            }
        }

        /// <summary>
        /// Gets the mode of the Student t-distribution.
        /// </summary>
        public double Mode
        {
            get { return _location; }
        }

        /// <summary>
        /// Gets the median of the Student t-distribution.
        /// </summary>
        public double Median
        {
            get { return _location; }
        }

        /// <summary>
        /// Gets the minimum of the Student t-distribution.
        /// </summary>
        public double Minimum
        {
            get { return Double.NegativeInfinity; }
        }

        /// <summary>
        /// Gets the maximum of the Student t-distribution.
        /// </summary>
        public double Maximum
        {
            get { return Double.PositiveInfinity; }
        }

        /// <summary>
        /// Computes the cumulative distribution (CDF) of the distribution at x, i.e. P(X ≤ x).
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative distribution function.</param>
        /// <returns>the cumulative distribution at location <paramref name="x"/>.</returns>
        public double CumulativeDistribution(double x)
        {
            return CDF(_location, _scale, _freedom, x);
        }

        /// <summary>
        /// Computes the cumulative distribution (CDF) of the distribution at x, i.e. P(X ≤ x).
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative distribution function.</param>
        /// <param name="location">The location (μ) of the distribution.</param>
        /// <param name="scale">The scale (σ) of the distribution. Range: σ > 0.</param>
        /// <param name="freedom">The degrees of freedom (ν) for the distribution. Range: ν > 0.</param>
        /// <returns>the cumulative distribution at location <paramref name="x"/>.</returns>
        /// <seealso cref="CumulativeDistribution"/>
        public static double CDF(double location, double scale, double freedom, double x)
        {
            if (scale <= 0.0 || freedom <= 0.0) throw new ArgumentException("InvalidDistributionParameters");

            // TODO JVG we can probably do a better job for Cauchy special case
            if (Double.IsPositiveInfinity(freedom)) return Normal.CDF(location, scale, x);

            var k = (x - location)/scale;
            var h = freedom/(freedom + (k*k));
            var ib = 0.5*SpecialFunctions.BetaRegularized(freedom/2.0, 0.5, h);
            return x <= location ? ib : 1.0 - ib;
        }

    }
}
