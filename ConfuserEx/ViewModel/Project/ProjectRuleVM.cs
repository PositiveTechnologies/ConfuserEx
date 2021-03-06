﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Confuser.Core;
using Confuser.Core.Project;
using Microsoft.Extensions.Logging.Abstractions;

namespace ConfuserEx.ViewModel {
	internal interface IRuleContainer {
		IList<ProjectRuleVM> Rules { get; }
	}

	public class ProjectRuleVM : ViewModelBase, IViewModel<Rule> {
		readonly ProjectVM parent;
		readonly Rule rule;
		string error;
		IPattern exp;

		public ProjectRuleVM(ProjectVM parent, Rule rule) {
			this.parent = parent;
			this.rule = rule;

			ObservableCollection<ProjectSettingVM<IProtection>> protections =
				Utils.Wrap(rule, setting => new ProjectSettingVM<IProtection>(parent, setting));
			protections.CollectionChanged += (sender, e) => parent.IsModified = true;
			Protections = protections;

			ParseExpression();
		}

		public ProjectVM Project {
			get { return parent; }
		}

		public string Pattern {
			get { return rule.Pattern; }
			set {
				if (SetProperty(rule.Pattern != value, val => rule.Pattern = val, value, "Pattern")) {
					parent.IsModified = true;
					ParseExpression();
				}
			}
		}

		public IPattern Expression {
			get { return exp; }
			set { SetProperty(ref exp, value, "Expression"); }
		}

		public string ExpressionError {
			get { return error; }
			set { SetProperty(ref error, value, "ExpressionError"); }
		}

		public ProtectionPreset Preset {
			get { return rule.Preset; }
			set {
				if (SetProperty(rule.Preset != value, val => rule.Preset = val, value, "Preset"))
					parent.IsModified = true;
			}
		}

		public bool Inherit {
			get { return rule.Inherit; }
			set {
				if (SetProperty(rule.Inherit != value, val => rule.Inherit = val, value, "Inherit"))
					parent.IsModified = true;
			}
		}

		public IList<ProjectSettingVM<IProtection>> Protections { get; private set; }

		Rule IViewModel<Rule>.Model {
			get { return rule; }
		}

		void ParseExpression() {
			if (Pattern == null)
				return;
			IPattern expression;
			try {
				expression = PatternParser.Parse(Pattern, NullLogger.Instance);
				ExpressionError = null;
			}
			catch (Exception e) {
				ExpressionError = e.Message;
				expression = null;
			}

			Expression = expression;
		}
	}
}
