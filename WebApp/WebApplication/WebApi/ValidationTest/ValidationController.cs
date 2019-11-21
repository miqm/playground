using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Mime;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using FluentValidation.AspNetCore;
using FluentValidation.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;

namespace WebApi.ValidationTest
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValidationController : ControllerBase
    {
        private readonly ApiBehaviorOptions _apiBehaviourOptions;

        public ValidationController(IValidator<DataEntity> validator, IMapper mapper, IOptions<ApiBehaviorOptions> options)
        {
            Validator = validator;
            Mapper = mapper;
            _apiBehaviourOptions = options.Value;
        }

        private IMapper Mapper { get; }

        private IValidator<DataEntity> Validator { get; }

        [HttpGet("get")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult Get()
        {
            return NotFound();
        }
        [HttpPut("data")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult CreateOrUpdate(DataInputDto input)
        {
            var obj = Mapper.Map<DataEntity>(input);
            var vResult = Validator.Validate(obj);
            var model = new ModelStateDictionary();
            vResult.AddToModelState(model, null);
            return Conflict(model);
        }

        public override ConflictObjectResult Conflict(ModelStateDictionary modelState)
        {
            var problemDetails = new ValidationProblemDetails(modelState)
            {
                Status = StatusCodes.Status409Conflict,
                Type = _apiBehaviourOptions.ClientErrorMapping[StatusCodes.Status409Conflict].Link
            };
            SetTraceId(HttpContext, problemDetails);

            var result  = base.Conflict(problemDetails);
            result.ContentTypes.Add("application/problem+json");
            result.ContentTypes.Add("application/problem+xml");
            return result;
        }
        internal static void SetTraceId(HttpContext httpContext, ProblemDetails problemDetails)
        {
            var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;
            problemDetails.Extensions["traceId"] = traceId;
        }
    }

    public class DataInputDto
    {
        [Required]
        public int? Id { get; set; }
        public string Name { get; set; }
        public string Other { get; set; }
        public SubDataInputDto Sub { get; set; }
    }

    public class DataEntity
    {
        public int DataId { get; set; }
        public string Name { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int SubDataId { get; set; }
    }
    public class SubDataInputDto {
        public int Id { get; set; }

    }

    public class DataProfile : Profile
    {
        public DataProfile()
        {
            CreateMap<DataInputDto, DataEntity>()
                .ForMember(s => s.DataId, o => o.MapFrom(d => d.Id))
                .ForMember(s => s.SubDataId, o => o.MapFrom(d => d.Sub.Id))
                .ForMember(s => s.CreatedOn, o => o.Ignore());
        }
    }

    public class EntityAbstractValidator<TDestination, TSource> : AbstractValidator<TDestination> where TDestination : class where TSource : class
    {
        private readonly IMapper _mapper;

        public EntityAbstractValidator(IMapper mapper)
        {
            _mapper = mapper;
        }
        
        private static IEnumerable<MemberInfo> PropertyChainFromExpression(LambdaExpression expression)
        {
            var members = new Stack<MemberInfo>();

            var getMemberExp = new Func<Expression, MemberExpression>(toUnwrap => {
                if (toUnwrap is UnaryExpression)
                {
                    return ((UnaryExpression)toUnwrap).Operand as MemberExpression;
                }

                return toUnwrap as MemberExpression;
            });

            var memberExp = getMemberExp(expression.Body);

            while (memberExp != null)
            {
                members.Push(memberExp.Member);
                memberExp = getMemberExp(memberExp.Expression);
            }

            return members;
        }

        private string GetSourcePropertyNameFromMapper<TProperty>(Expression<Func<TDestination, TProperty>> expression)
        {
            var destinationChain = PropertyChainFromExpression(expression).Select(m => m.Name).ToList();
            var map = _mapper.ConfigurationProvider.FindTypeMapFor(typeof(TSource), typeof(TDestination));
            var memberMap = map.MemberMaps.FirstOrDefault(m => destinationChain.Contains(m.DestinationName));
            if (memberMap == null)
                return null;
            return memberMap.CustomMapExpression != null ? string.Join('.', PropertyChainFromExpression(memberMap.CustomMapExpression).Select(m => m.Name)) : memberMap.SourceMember?.Name;
        }


        public IRuleBuilderInitial<TDestination, TProperty> RuleForWithSourceProperty<TProperty>(Expression<Func<TDestination, TProperty>> expression)
        {
            return RuleFor(expression)
                .Configure(c =>
                {
                    if (c.PropertyName != null)
                    {
                        c.PropertyName = GetSourcePropertyNameFromMapper(expression) ?? c.PropertyName;
                    }
                });
        }
        public IRuleBuilderInitial<TDestination, TProperty> RuleForWithSourceProperty<TProperty, TSourceProperty>(Expression<Func<TDestination, TProperty>> expression, Expression<Func<TSource, TSourceProperty>> sourceExpression)
        {
            return RuleFor(expression)
                .Configure(c =>
                {
                    if (c.PropertyName != null)
                    {
                        c.PropertyName = string.Join('.', PropertyChainFromExpression(sourceExpression).Select(m => m.Name)) ?? c.PropertyName;
                    }
                });
        }
    }

    public class DataEntityValidator : EntityAbstractValidator<DataEntity, DataInputDto>
    {
        public DataEntityValidator(IMapper mapper) : base(mapper)
        {
            RuleForWithSourceProperty(c => c.SubDataId)
                .GreaterThan(0);
            RuleForWithSourceProperty(c => c.DataId)
                .GreaterThan(0);
            RuleForWithSourceProperty(c => c.CreatedOn, s => s.Other)
                .NotNull();
        }
    }
}