﻿using Benchmark.Models;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Jobs;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace Benchmark
{
    [CoreJob]
    [Config(typeof(BenchmarkConfig))]
    public class EfCore2VsDapper
    {
        [Benchmark]
        public Bets EfGetBetById()
        {
            using (var db = new northwindContext())
            {
                long betId = 100;
                return db.Bets.Find(betId);
            }
        }

        [Benchmark]
        public Bets EfGetBetByIdNoTracking()
        {
            using (var db = new northwindContext())
            {
                long betId = 100;
                return db.Bets.AsNoTracking().First(p => p.BetId == betId);
            }
        }

        [Benchmark]
        public Bets EfGetBetByIdWithSql()
        {
            using (var db = new northwindContext())
            {
                long betId = 100;
                return db.Bets
                         .AsNoTracking()
                         .FromSql("select * from Bets where BetId = @betId", new SqlParameter("betId", betId)).First();
            }
        }

        [Benchmark]
        public Bets DapperGetBetById()
        {
            using (var conn = ConnectionFactory.GetConnection())
            {
                var bet = conn.QueryFirst<Bets>("select * from Bets where BetId = @betId", new { betId = 100 });

                return bet;
            }
        }

        [Benchmark]
        public Members EfGetMemberBetsByMemberId()
        {
            using (var db = new northwindContext())
            {
                long memberId = 200;
                return db.Members
                         .Include(p => p.Bets)
                         .First(p => p.MemberId == memberId);
            }
        }

        [Benchmark]
        public Members EfGetMemberBetsByMemberIdNoTracking()
        {
            using (var db = new northwindContext())
            {
                long memberId = 200;
                return db.Members
                         .Include(p => p.Bets)
                         .AsNoTracking()
                         .First(p => p.MemberId == memberId);
            }
        }

        [Benchmark]
        public Members DapperGetMemberBetsByMemberId()
        {
            using (var conn = ConnectionFactory.GetConnection())
            {
                long memberId = 200;
                var sql = @"select * from Members m 
                            left join Bets b on m.MemberId = b.MemberId 
                            where m.MemberId = @memberId";

                var memberLookup = new Dictionary<long, Members>();

                return conn.Query<Members, Bets, Members>(sql,
                                                          (member, bet) =>
                                                          {
                                                              Members m;
                                                              if (!memberLookup.TryGetValue(member.MemberId, out m))
                                                              {
                                                                  m = member;
                                                                  memberLookup.Add(member.MemberId, m);
                                                              }
                                                              bet.Member = m;
                                                              m.Bets.Add(bet);
                                                              return m;
                                                          },
                                                          param: new { memberId = memberId },
                                                          splitOn: "BetId")
                                                          .First();
            }
        }

        [Benchmark]
        public void EfAddOneHundredNewBets()
        {
            using (var db = new northwindContext())
            {
                for (int i = 0; i < 100; i++)
                {
                    db.Bets.Add(new Bets { MemberId = 1, StakeAmount = 100, DateCreated = DateTime.Now, DateUpdated = DateTime.Now });
                    db.SaveChanges();
                }
            }
        }

        [Benchmark]
        public void DapperAddOneHundredNewBets()
        {
            using (var conn = ConnectionFactory.GetConnection())
            {
                for (int i = 0; i < 100; i++)
                {
                    conn.Insert(new Bets { MemberId = 1, StakeAmount = 100, DateCreated = DateTime.Now, DateUpdated = DateTime.Now });
                }
            }
        }

        [Benchmark]
        public void EfUpdateOneHundredBets()
        {
            using (var db = new northwindContext())
            {
                for (int i = 0; i < 100; i++)
                {
                    db.Update(new Bets { BetId = i, MemberId = 1, StakeAmount = 99, DateCreated = DateTime.Now, DateUpdated = DateTime.Now });
                    db.SaveChanges();
                }
            }
        }

        [Benchmark]
        public void DapperUpdateOneHundredBets()
        {
            using (var conn = ConnectionFactory.GetConnection())
            {
                for (int i = 0; i < 100; i++)
                {
                    conn.Update(new Bets { BetId = i, MemberId = 1, StakeAmount = 49, DateCreated = DateTime.Now, DateUpdated = DateTime.Now });                    
                }
            }
        }
    }
}