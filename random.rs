#[derive(Copy,Clone)]
struct Random([i32;56],i32,i32);//{seedarray,intext,intextp}
impl Random{
    pub fn new(seed:i32)->Self{
        //int num = 161803398 - Math.Abs(Seed);
        let mut num = 161803398 - seed.abs();
        //this.SeedArray[55] = num;
        let mut ret=Self([0;56],0,31);
        ret.0[55]=num;
        //int num2 = 1;
        let mut num2=1;
        //for (int i = 1; i < 55; i++)
        for i in 1..55{
            //println!("{:?}",num2);
            //int num3 = 21 * i % 55;
            let num3=21*i%55;
            //this.SeedArray[num3] = num2;
            ret.0[num3 as usize]=num2;
            //num2 = num - num2;
            num2=num-num2;
            //if (num2 < 0)
            if num2 < 0{
                //num2 += int.MaxValue;
                num2+=2147483647;
            }
            //num = this.SeedArray[num3];
            num = ret.0[num3 as usize];
        }
        //for (int j = 1; j < 5; j++)
        for j in 1..5{
            //for (int k = 1; k < 56; k++)
            for k in 1..56{
                //this.SeedArray[k] -= this.SeedArray[1 + (k + 30) % 55];
                ret.0[k]-=ret.0[1+(k+30)%55];
                //if (this.SeedArray[k] < 0)
                if ret.0[k]<0{
                    //this.SeedArray[k] += int.MaxValue;
                    ret.0[k]+=2147483647;
                }
            }
        }
        //this.inext = 0;
        //this.inextp = 31;
        ret
    }

    //protected virtual double Sample()
    pub fn sample(&mut self)->f64{
        self.1+=1;
        //if (++this.inext >= 56)
        if self.1>=56{
            //this.inext = 1;
            self.1=1;
        }
        self.2+=1;
        //if (++this.inextp >= 56)
        if self.2>=56{
            self.2 = 1;
        }
        //int num = this.SeedArray[this.inext] - this.SeedArray[this.inextp];
        let mut num = self.0[self.1 as usize] - self.0[self.2 as usize];
        //if (num < 0)
        if num<0{
            //num += int.MaxValue;
            num+=2147483647;
        }
        //this.SeedArray[this.inext] = num;
        self.0[self.1 as usize]=num;
        num as f64 * 4.6566128752457969E-10
    }

    //public virtual int Next()
    pub fn next(&mut self)->i32{
        //return (int)(this.Sample() * 2147483647.0);
        (self.sample()*2147483647.0) as i32
    }
/*
		// Token: 0x0600135D RID: 4957 RVA: 0x0004D884 File Offset: 0x0004BA84
		public virtual int Next(int maxValue)
		{
			if (maxValue < 0)
			{
				throw new ArgumentOutOfRangeException(Locale.GetText("Max value is less than min value."));
			}
			return (int)(this.Sample() * (double)maxValue);
		}

		// Token: 0x0600135E RID: 4958 RVA: 0x0004D8A8 File Offset: 0x0004BAA8
		public virtual int Next(int minValue, int maxValue)
		{
			if (minValue > maxValue)
			{
				throw new ArgumentOutOfRangeException(Locale.GetText("Min value is greater than max value."));
			}
			uint num = (uint)(maxValue - minValue);
			if (num <= 1U)
			{
				return minValue;
			}
			return (int)((ulong)((uint)(this.Sample() * num)) + (ulong)((long)minValue));
		}
		// Token: 0x0600135F RID: 4959 RVA: 0x0004D8EC File Offset: 0x0004BAEC
		public virtual void NextBytes(byte[] buffer)
		{
			if (buffer == null)
			{
				throw new ArgumentNullException("buffer");
			}
			for (int i = 0; i < buffer.Length; i++)
			{
				buffer[i] = (byte)(this.Sample() * 256.0);
			}
		}

*/
		// Token: 0x06001360 RID: 4960 RVA: 0x0004D934 File Offset: 0x0004BB34
    //public virtual double NextDouble()
    pub fn next_double(&mut self)->f64{
        //return this.Sample();
        self.sample()
    }

    // Token: 0x0400059E RID: 1438
    const MBIG:i32 = 2147483647;

    // Token: 0x0400059F RID: 1439
    const MSEED:i32 = 161803398;

    // Token: 0x040005A0 RID: 1440
    const MZ:i32 = 0;

    // Token: 0x040005A1 RID: 1441
    //private int inext;

    // Token: 0x040005A2 RID: 1442
    //private int inextp;

    // Token: 0x040005A3 RID: 1443
    //private int[] SeedArray = new int[56];
}
/*fn main(){
    let mut t=Random::new(123);
    for i in 0..100{
        println!("{:?}",t.next());
    }
}*/
