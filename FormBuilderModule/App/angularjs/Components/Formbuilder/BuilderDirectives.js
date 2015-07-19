angular.module('formbuilder')
	.directive('formbuilder',function(){
		return {
			templateUrl:'/Templates/Builder.htm',
			restrict:'E',
			scope: {
				FormBuilderID: "@id"
			},
			controllerAs: 'Builder',
			controller: ['$scope','$window','formAPI','redirectPath',function($scope,$window,formAPI,redirectPath){
				var builder = this;
				var config = {
					Field: function() {
						this.Type = $scope.newFieldType;
						this.FormID = $scope.model.ID;
						switch (this.Type) {
							case 'textbox':
							case 'textarea':
							case 'text':
								this.hasOptions = false;
								break;
							case 'select':
							case 'checkbox':
							case 'radio':
								this.hasOptions = true;
								this.Options = [];
								break;
							case '':
								throw new Error("Please select a field type first.");
								break;
							default:
								throw new Error("Field type not supported. Type given: " + this.Type);
								break;
						}

						if (this.Type == 'text')
							this.Values = [new config.Value(this)];
						else
							this.Values = [];

						if($scope.model.Fields.length > 0) {
							this.SortOrder = ($scope.model.Fields.reduce(function(prev,curr){
							    if (curr.SortOrder > prev)
							        return curr.SortOrder;
								else
									return prev;
							},0))+1;
						} else {
						    this.SortOrder = 1;
						}
					},
					Option: function(field) {
						this.FieldID = field.ID;
						//TODO: Sort order neeeds to be fixed so that it iss based on the order of array server-side
						if(field.Options.length == 0) {
							this.SortOrder = 0;
						} else {
						    this.SortOrder = (field.Options.reduce(function (prev, curr) {
						        if (curr.SortOrder > prev)
						            return curr.SortOrder;
								else
									return prev;
							},0))+1;
						}
						this.Value = "";
					},
					Value: function(field) {
						this.FieldID = field.ID;
						this.ID = null;
						this.Content = "";
					}
				};

				function Form() {
					this.ID = null;
					this.Name = null;
					this.Description = null;
					this.Fields = [];
				}

				$scope.model = new Form();
				$scope.newFieldType = "";

				$scope.addNewField = function() {
					try {
						var newField = new config.Field();
						$scope.model.Fields.push(newField);
					} catch (error) {
						//handle error message
						throw error;
						delete newField;
					}
					$scope.newFieldType = "";
				};

				$scope.removeField = function(field) {
					//Do confirmation here.
					$scope.model.Fields = $scope.model.Fields.filter(function(element) {
						return element !== field;
					});
				};

				$scope.fieldChanged = function() {
					console.log($scope.model);
				};

				$scope.addNewOption = function(field) {
					//for debug only consolidate later
					var newOption = new config.Option(field);
					field.Options.push(newOption);
				};

				$scope.removeOption = function(field,option) {
					field.Options = field.Options.filter(function(elem){
						return elem !== option;
					});
				};

				$scope.saveForm = function() {
					formAPI.save($scope.model,function(data) {
						console.log(data.result);
						if(data.result == "success") {
							$window.location.href = redirectPath;
						} else if (data.result == "error") {
							//Display Error Message
						} else {
							//Result unknown.
						}
					});
				};

				$scope.hasOptions = function (type) {
					switch(type) {
						case 'textbox':
						case 'textarea':
						case 'text':
							return false;
						case 'select':
						case 'checkbox':
						case 'radio':
							return true;
						case '':
							return false;
							break;
						default:
							return false;
					}
				};

				// if(typeof $scope.FormBuilderID != "undefined") {
					console.log($scope.FormBuilderID);
				// }

				if(typeof $scope.FormBuilderID != "undefined") {
					formAPI.get($scope.FormBuilderID,function(data) {
						console.log(data);
						$scope.model = data;
					});
				}
			}]
		}
	})
	.directive('formdisplay',function(){
		return {
			templateUrl:'/Templates/Displayer.htm',
			restrict:'E',
			scope: {
				FormBuilderID: "@id"
			},
			controllerAs: 'Displayer',
			controller: ['$scope','formAPI',function($scope,formAPI){
				var builder = this;

				console.log($scope.FormBuilderID);

				if(typeof $scope.FormBuilderID != "undefined") {
					formAPI.get($scope.FormBuilderID,function(data) {
						console.log(data);
						$scope.model = data;
					});
				}
			}]
		}
	});;