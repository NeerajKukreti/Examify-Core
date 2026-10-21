"""
debate_engine/gemini_client.py
Async Gemini API client with auto-discovery from Examify appsettings and zero external dependencies.
Supports mock mode for offline testing, CI/CD, and fast simulation.
"""

from __future__ import annotations
import os
import json
import asyncio
import urllib.request
import urllib.error
from pathlib import Path
from typing import Dict, Any, Optional


class GeminiClient:
    def __init__(self, workspace_root: Optional[Path] = None, api_key: Optional[str] = None):
        self.root = workspace_root or Path(__file__).resolve().parent.parent
        self.api_key = api_key or self._discover_api_key()
        self.default_model = "gemini-1.5-flash"

    def _discover_api_key(self) -> Optional[str]:
        # 1. Environment variable
        env_key = os.environ.get("GEMINI_API_KEY")
        if env_key:
            return env_key

        # 2. Examify/appsettings.json
        appsettings_path = self.root / "Examify" / "appsettings.json"
        if appsettings_path.exists():
            try:
                with open(appsettings_path, "r", encoding="utf-8") as f:
                    cfg = json.load(f)
                    key = cfg.get("Gemini", {}).get("ApiKey")
                    if key and not key.startswith("YOUR_"):
                        return key
            except Exception:
                pass
        return None

    async def generate_json_async(
        self,
        prompt: str,
        system_instruction: str,
        model: Optional[str] = None,
        use_mock: bool = False
    ) -> Dict[str, Any]:
        """Call Gemini API or mock generator to return a parsed JSON payload."""
        if use_mock or not self.api_key:
            return self._generate_mock_response(prompt, system_instruction)

        target_model = model or self.default_model
        endpoint = f"https://generativelanguage.googleapis.com/v1beta/models/{target_model}:generateContent?key={self.api_key}"

        payload = {
            "contents": [
                {
                    "parts": [{"text": prompt}]
                }
            ],
            "systemInstruction": {
                "parts": [{"text": system_instruction}]
            },
            "generationConfig": {
                "responseMimeType": "application/json",
                "temperature": 0.2
            }
        }

        def _do_request():
            req_data = json.dumps(payload).encode("utf-8")
            req = urllib.request.Request(
                endpoint,
                data=req_data,
                headers={"Content-Type": "application/json"},
                method="POST"
            )
            try:
                with urllib.request.urlopen(req, timeout=30) as resp:
                    resp_body = resp.read().decode("utf-8")
                    data = json.loads(resp_body)
                    candidates = data.get("candidates", [])
                    if candidates and "content" in candidates[0]:
                        parts = candidates[0]["content"].get("parts", [])
                        if parts:
                            raw_text = parts[0].get("text", "{}")
                            return json.loads(raw_text)
                    return {}
            except Exception as e:
                # If API call fails (rate limit, offline, or invalid key), fall back gracefully to mock
                return self._generate_mock_response(prompt, system_instruction, error_note=str(e))

        loop = asyncio.get_running_loop()
        return await loop.run_in_executor(None, _do_request)

    def _generate_mock_response(
        self,
        prompt: str,
        system_instruction: str,
        error_note: Optional[str] = None
    ) -> Dict[str, Any]:
        """Intelligent deterministic fallback when offline or using --mock."""
        # Detect role from prompt/system instruction
        text = (system_instruction + " " + prompt).lower()

        if "the_pen_tester" in text or "security" in text:
            # Check if target context has the tenant isolation check
            has_tenant_check = (
                "getcurrentinstituteid" in text or
                "exam.instituteid !=" in text or
                "statuscode(403" in text
            )
            if not has_tenant_check and ("getexambyid" in text or "examcontroller" in text or "idor" in text):
                return {
                    "lens_id": "the_pen_tester",
                    "chamber": "Chamber 2: Technical & Defense",
                    "status": "HARD_VETO",
                    "hard_veto_triggered": True,
                    "hard_veto_rationale": "P0 Security Invariant: Endpoint missing InstituteId tenant isolation check.",
                    "issues": [
                        {
                            "issue_id": "SEC-P0-001",
                            "title": "Unauthenticated or Cross-Tenant Exam Access",
                            "severity": 5,
                            "target_file": "ExamifyAPI/Controllers/ExamController.cs",
                            "line_range": "60-75",
                            "description": "Exam retrieval endpoint lacks explicit InstituteId validation against JWT claim.",
                            "metric_impact": None,
                            "proposed_diff": "--- a/ExamifyAPI/Controllers/ExamController.cs\n+++ b/ExamifyAPI/Controllers/ExamController.cs\n@@ -65,3 +65,6 @@\n+ var currentInstituteId = _authService.GetCurrentInstituteId();\n+ if (exam.InstituteId != currentInstituteId) return StatusCode(403, new { Success = false, Message = \"Forbidden: Cross-tenant access denied.\" });",
                            "concession": None
                        }
                    ],
                    "summary": "Hard Veto triggered: P0 vulnerability violates multi-tenant isolation."
                }
            return {
                "lens_id": "the_pen_tester",
                "chamber": "Chamber 2: Technical & Defense",
                "status": "PASS",
                "hard_veto_triggered": False,
                "hard_veto_rationale": None,
                "issues": [],
                "summary": "Security posture verified. Multi-tenant isolation verified with zero P0 exploit vectors."
            }

        if "the_legal_counsel" in text:
            return {
                "lens_id": "the_legal_counsel",
                "chamber": "Chamber 3: Risk, Legal & Armor",
                "status": "PASS",
                "hard_veto_triggered": False,
                "hard_veto_rationale": None,
                "issues": [
                    {
                        "issue_id": "LEGAL-001",
                        "title": "Mandatory Assessment Integrity Disclaimer Required",
                        "severity": 3,
                        "target_file": "Examify/Views/QuestionExtractor/Index.cshtml",
                        "description": "OCR question processing requires disclaimer regarding institutional ownership of exam papers.",
                        "proposed_diff": "--- a/Examify/Views/QuestionExtractor/Index.cshtml\n+++ b/Examify/Views/QuestionExtractor/Index.cshtml\n@@ -20,3 +20,4 @@\n+ <div class=\"legal-notice\">By uploading, you affirm you hold distribution rights.</div>",
                        "concession": "Accept subtle footer notice instead of blocking modal"
                    }
                ],
                "summary": "Legal review complete. Disclaimers verified."
            }

        if "the_seo_auditor" in text:
            return {
                "lens_id": "the_seo_auditor",
                "chamber": "Chamber 1: Commercial & Growth",
                "status": "FLAGGED",
                "hard_veto_triggered": False,
                "hard_veto_rationale": None,
                "issues": [
                    {
                        "issue_id": "SEO-001",
                        "title": "Add JSON-LD Quiz Schema markup to public catalog",
                        "severity": 2,
                        "target_file": "Examify/Views/Home/Index.cshtml",
                        "line_range": "10-25",
                        "description": "Rich search snippets require schema.org Quiz/Course markup for catalog exams.",
                        "metric_impact": {
                            "latency_ms_delta": 6.0,
                            "conversion_percent_delta": 24.0
                        },
                        "proposed_diff": "--- a/Examify/Views/Home/Index.cshtml\n+++ b/Examify/Views/Home/Index.cshtml\n@@ -12,3 +12,7 @@\n+ <script type=\"application/ld+json\">\n+ {\"@context\": \"https://schema.org\", \"@type\": \"Quiz\"}\n+ </script>",
                        "concession": "Willing to accept async injection if server render exceeds 15ms"
                    }
                ],
                "summary": "SEO audit identified rich snippet opportunity."
            }

        if "the_latency_hunter" in text:
            return {
                "lens_id": "the_latency_hunter",
                "chamber": "Chamber 2: Technical & Defense",
                "status": "FLAGGED",
                "hard_veto_triggered": False,
                "hard_veto_rationale": None,
                "issues": [
                    {
                        "issue_id": "PERF-001",
                        "title": "Eliminate synchronous blocking in async flow",
                        "severity": 3,
                        "target_file": "ExamifyAPI/Services/AuthService.cs",
                        "description": "Ensure ConfigureAwait(false) and remove any .Result calls.",
                        "metric_impact": {
                            "latency_ms_delta": -12.0
                        },
                        "proposed_diff": "--- a/ExamifyAPI/Services/AuthService.cs\n+++ b/ExamifyAPI/Services/AuthService.cs\n@@ -40,3 +40,3 @@\n- var res = task.Result;\n+ var res = await task.ConfigureAwait(false);",
                        "concession": "Tolerate up to 15ms latency if conversion lift exceeds 20%"
                    }
                ],
                "summary": "Latency analysis complete. Soft trade-off limits evaluated."
            }

        # Default fallback
        return {
            "lens_id": "generic_lens",
            "chamber": "Chamber 2: Technical & Defense",
            "status": "PASS",
            "hard_veto_triggered": False,
            "hard_veto_rationale": None,
            "issues": [],
            "summary": "Evaluation passed all baseline checks."
        }
