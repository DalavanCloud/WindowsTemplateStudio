﻿// ******************************************************************
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THE CODE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH
// THE CODE OR THE USE OR OTHER DEALINGS IN THE CODE.
// ******************************************************************

using Microsoft.Templates.Core.Gen;
using Microsoft.Templates.UI.Resources;

namespace Microsoft.Templates.UI.ViewModels.NewItem
{
    public class WarningFileViewModel : BaseFileViewModel
    {
        public override FileType FileType => FileType.WarningFile;

        public WarningFileViewModel(GenerationWarning warning) : base(warning.FailedFileName)
        {
            DetailTitle = StringRes.ChangesSummaryDetailTitleMergeConflicts;
            DetailDescription = warning.Description;
            DetailExtendedInfo = warning.ExtendedInfo;
            Subject = warning.FileName;
        }
    }
}
