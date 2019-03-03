using System;
using System.Linq;
using GraphQL.Types;

namespace GraphQL.Server.Test.GraphQL.Models {

	public class Specie {
		public string AverageHeight { get; set; }
		public string AverageLifespan { get; set; }
		public string Classification { get; set; }
		public DateTime Created { get; set; }
		public string Designation { get; set; }
		public DateTime Edited { get; set; }
		public string EyeColors { get; set; }
		public IQueryable<People> Films { get; set; }
		public string HairColors { get; set; }
		public Planet Homeworld { get; set; }
		public int Id { get; set; }
		public string Language { get; set; }
		public string Name { get; set; }
		public IQueryable<People> People { get; set; }
		public string SkinColors { get; set; }
	}

	public class SpecieGraphType : ObjectGraphType<Specie> {

		public SpecieGraphType() {
			this.Name = nameof(Specie);
			this.Field(expression => expression.AverageHeight);
			this.Field(expression => expression.AverageLifespan);
			this.Field(expression => expression.Classification);
			this.Field(expression => expression.Created);
			this.Field(expression => expression.Designation);
			this.Field(expression => expression.Edited);
			this.Field(expression => expression.EyeColors);
			this.Field<ListGraphType<FilmGraphType>>("films");
			this.Field(expression => expression.HairColors);
			this.Field(expression => expression.Homeworld);
			this.Field(expression => expression.Id);
			this.Field(expression => expression.Language);
			this.Field(expression => expression.Name);
			this.Field<ListGraphType<PeopleGraphType>>("people");
			this.Field(expression => expression.SkinColors);
		}

	}

}
