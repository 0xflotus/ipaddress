
using System;
using System.Numerics;
using System.Text;
using System.Collections.Generic;

namespace ipaddress
{
  //  Ac
  ///  It is usually identified as a IPv4 mapped IPv6 address, a particular
  ///  IPv6 address which aids the transition from IPv4 to IPv6. The
  ///  structure of the address is
  ///
  ///    ::ffff:w.y.x.z
  ///
  ///  where w.x.y.z is a normal IPv4 address. For example, the following is
  ///  a mapped IPv6 address:
  ///
  ///    ::ffff:192.168.100.1
  ///
  ///  IPAddress is very powerful in handling mapped IPv6 addresses, as the
  ///  IPv4 portion is stored internally as a normal IPv4 object. Let's have
  ///  a look at some examples. To create a new mapped address, just use the
  ///  class builder itself
  ///
  ///    ip6 = IPAddress::IPv6::Mapped.new "::ffff:172.16.10.1/128"
  ///
  ///  or just use the wrapper method
  ///
  ///    ip6 = IPAddress "::ffff:172.16.10.1/128"
  ///
  ///  Let's check it's really a mapped address:
  ///
  ///    ip6.mapped?
  ///      ///  true
  ///
  ///    ip6.to_string
  ///      ///  "::FFFF:172.16.10.1/128"
  ///
  ///  Now with the +ipv4+ attribute, we can easily access the IPv4 portion
  ///  of the mapped IPv6 address:
  ///
  ///    ip6.ipv4.address
  ///      ///  "172.16.10.1"
  ///
  ///  Internally, the IPv4 address is stored as two 16 bits
  ///  groups. Therefore all the usual methods for an IPv6 address are
  ///  working perfectly fine:
  ///
  ///    ip6.to_hex
  ///      ///  "00000000000000000000ffffac100a01"
  ///
  ///    ip6.address
  ///      ///  "0000:0000:0000:0000:0000:ffff:ac10:0a01"
  ///
  ///  A mapped IPv6 can also be created just by specify the address in the
  ///  following format:
  ///
  ///    ip6 = IPAddress "::172.16.10.1"
  ///
  ///  That is, two colons and the IPv4 address. However, as by RFC, the ffff
  ///  group will be automatically added at the beginning
  ///
  ///    ip6.to_string
  ///      => "::ffff:172.16.10.1/128"
  ///
  ///  making it a mapped IPv6 compatible address.
  ///
  ///
  ///  Creates a new IPv6 IPv4-mapped address
  ///
  ///    ip6 = IPAddress::IPv6::Mapped.new "::ffff:172.16.10.1/128"
  ///
  ///    ipv6.ipv4.class
  ///      ///  IPAddress::IPv4
  ///
  ///  An IPv6 IPv4-mapped address can also be created using the
  ///  IPv6 only format of the address:
  ///
  ///    ip6 = IPAddress::IPv6::Mapped.new "::0d01:4403"
  ///
  ///    ip6.to_string
  ///      ///  "::ffff:13.1.68.3"
  ///
  class Ipv6Mapped
  {
    public static Result<IPAddress> create(String str) {
      var ret = IPAddress.Split_at_slash(str);
      var split_colon = ret.addr.Split(new string[] { ":" }, StringSplitOptions.None);
      if (split_colon.Length <= 1)
      {
        // println!("---1");
        return Result<IPAddress>.Err("not mapped format-1: <<str>>");
      }
      var netmask = "";
      if (ret.netmask != null)
      {
        netmask = string.Format("/{0}", ret.netmask);
      }
      var ipv4_str = split_colon[split_colon.Length - 1];
        if (IPAddress.is_valid_ipv4(ipv4_str))
      {
        var ipv4 = IPAddress.parse(string.Format("{0}{1}", ipv4_str, netmask));
        if (ipv4.isErr())
        {
          // println!("---2");
          return ipv4;
        }
        //mapped = Some(ipv4.unwrap());
        var addr = ipv4.unwrap();
        var ipv6_bits = IpBits.V6;
            var part_mod = ipv6_bits.part_mod;
        var up_addr = addr.host_address;
        var down_addr = addr.host_address;

        var rebuild_ipv6 = new StringBuilder();
            var colon = "";
        for (var i = 0; i < split_colon.Length - 1; i++)
        {
          rebuild_ipv6.Append(colon);
          rebuild_ipv6.Append(split_colon[i]);
          colon = ":";
        }
        rebuild_ipv6.Append(colon);
        var rebuild_ipv4 = string.Format("{0}:{1}/{2}",
          ((up_addr >> (int)IpBits.V6.part_bits) % part_mod).ToString("x"),
          (down_addr % part_mod).ToString("x"),
          ipv6_bits.bits - addr.prefix.host_prefix());
        rebuild_ipv6.Append(rebuild_ipv4);
        var r_ipv6 = IPAddress.parse(rebuild_ipv6.ToString());
        if (r_ipv6.isErr())
        {
          // println!("---3|{}", &rebuild_ipv6);
          return r_ipv6;
        }
        if (r_ipv6.unwrap().is_mapped())
        {
          return r_ipv6;
        }
        var ipv6 = r_ipv6.unwrap();
        var p96bit = ipv6.host_address >> (32);
        if (p96bit != 0)
        {
          // println!("---4|{}", &rebuild_ipv6);
          return Result<IPAddress>.Err("is not a mapped address:<<rebuild_ipv6>>");
        }
        {
          var rr_ipv6 = IPAddress.parse(string.Format("::ffff:{0}", rebuild_ipv4));
          if (rr_ipv6.isErr())
          {
            //println!("---3|{}", &rebuild_ipv6);
            return rr_ipv6;
          }
          return rr_ipv6;
        }
      }
      return Result<IPAddress>.Err("unknown mapped format:<<str>>");
    }
  }
}