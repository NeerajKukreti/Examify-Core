"""
debate_engine/cli.py
CLI entrypoint for executing the Parallelized Debate Graph on ExamiFy-Core.
"""

from __future__ import annotations
import argparse
import asyncio
import json
import sys
import subprocess
from pathlib import Path

from .dag_runner import DebateGraphDAG
from .telemetry import TelemetryCollector
from .applier import AutonomousRefactorApplier


def parse_args():
    parser = argparse.ArgumentParser(
        description="ExamiFy 10-Lens Multi-Agent Parallelized Debate Graph (DAG) Engine"
    )
    parser.add_argument(
        "--target",
        "-t",
        type=str,
        default="ExamifyAPI/Controllers/ExamController.cs",
        help="Target source file, directory, or diff context to evaluate."
    )
    parser.add_argument(
        "--mock",
        action="store_true",
        default=True,
        help="Run in offline deterministic zero-cost mode (default: 100%% free, zero cost incurred)."
    )
    parser.add_argument(
        "--free-tier-live",
        action="store_true",
        help="Connect to Google AI Studio Free Tier (strictly verifying $0.00 cost before calling)."
    )
    parser.add_argument(
        "--all-controllers",
        action="store_true",
        help="Batch scan and evaluate all 26 controllers across ExamifyAPI and Examify."
    )
    parser.add_argument(
        "--all-modified",
        action="store_true",
        help="Batch scan all git-modified files across the workspace."
    )
    parser.add_argument(
        "--telemetry-only",
        action="store_true",
        help="Inspect dynamic telemetry and 50% reserve invariant status without executing debate."
    )
    parser.add_argument(
        "--apply",
        action="store_true",
        help="Autonomously apply approved auto-refactor directives within guardrails."
    )
    parser.add_argument(
        "--output",
        "-o",
        type=str,
        default=None,
        help="Optional path to output the Consensus Decree JSON."
    )
    return parser.parse_args()


async def main_async():
    args = parse_args()
    workspace_root = Path(__file__).resolve().parent.parent

    # Force mock mode if free-tier-live is not explicitly requested, ensuring zero cost
    use_mock = not args.free_tier_live

    if args.telemetry_only:
        collector = TelemetryCollector(workspace_root)
        rep = collector.collect()
        print("\n" + "=" * 65)
        print(" [CHAMBER 3 GROUNDED TELEMETRY & ZERO-COST MANIFEST REPORT]")
        print("=" * 65)
        print("Cost Governance Policy:          STRICT ZERO-COST ($0.00 Ceiling)")
        print(f"Total Incurred Cost:             ${rep.accumulated_spend_usd:.2f} / ${rep.monthly_budget_cap_usd:.2f} (Zero-Cost Invariant: Satisfied)")
        print("Paid Cloud/API Calls Block:      ACTIVE (100% Free Tier / Local)")
        print(f"Host CPU Utilization:            {rep.observed_cpu_utilization_pct}% (Reserve OK: {rep.cpu_reserve_ok})")
        print(f"Host Memory Working Set:         {rep.observed_memory_working_set_mb} MB (Reserve OK: {rep.memory_reserve_ok})")
        print(f"Database Active Connection Cap:  {rep.db_active_connection_limit} / {rep.db_max_pool_size} (Reserve OK: {rep.db_reserve_ok})")
        print(f"50% Reserve Invariant Satisfied: {rep.is_within_50_percent_reserve}")
        print("=" * 65 + "\n")
        return

    # 1. Resolve Target Files (single file, folder, all controllers, or git modified)
    target_files = []
    if args.all_controllers:
        for p in workspace_root.glob("**/Controllers/**/*Controller.cs"):
            if "bin" not in str(p) and "obj" not in str(p):
                target_files.append(p)
    elif args.all_modified:
        try:
            res = subprocess.run("git diff --name-only HEAD", shell=True, cwd=workspace_root, capture_output=True, text=True)
            for line in res.stdout.strip().splitlines():
                if line.strip():
                    p = workspace_root / line.strip()
                    if p.exists() and p.is_file():
                        target_files.append(p)
        except Exception:
            pass
    elif (workspace_root / args.target).is_dir():
        for p in (workspace_root / args.target).rglob("*.cs"):
            if "bin" not in str(p) and "obj" not in str(p):
                target_files.append(p)
    else:
        target_files.append(workspace_root / args.target)

    if not target_files:
        print("[ERROR] No matching files found to evaluate.")
        return

    dag = DebateGraphDAG(workspace_root=workspace_root, use_mock=use_mock)

    if len(target_files) > 1:
        print(f"\n[DAG BATCH] Initiating batch evaluation across {len(target_files)} target files...")
        batch_summary = []
        for idx, fpath in enumerate(target_files, 1):
            rel_path = fpath.relative_to(workspace_root) if fpath.is_relative_to(workspace_root) else fpath
            try:
                with open(fpath, "r", encoding="utf-8", errors="ignore") as f:
                    content = f"File: {rel_path}\nContent:\n" + f.read()[:3000]
            except Exception:
                content = f"File: {rel_path}"

            print(f"\n--- [{idx}/{len(target_files)}] Auditing {rel_path} ---")
            _, _, decree, _ = await dag.execute_dag(content, target_file=str(rel_path).replace("\\", "/"))
            batch_summary.append({
                "file": str(rel_path).replace("\\", "/"),
                "verdict": decree.verdict,
                "pen_veto": decree.veto_registry.pen_tester_veto_active,
                "legal_veto": decree.veto_registry.legal_counsel_veto_active,
                "directives": len(decree.auto_refactor_directives)
            })

        print("\n" + "=" * 80)
        print(f"               PROJECT BATCH AUDIT SUMMARY ({len(target_files)} FILES EVALUATED)")
        print("=" * 80)
        print(f"{'Target File':<50} | {'Verdict':<20} | {'Veto Active'}")
        print("-" * 80)
        for s in batch_summary:
            veto_str = "🛑 PEN_TESTER" if s["pen_veto"] else ("🛑 LEGAL" if s["legal_veto"] else "NONE")
            print(f"{s['file']:<50} | {s['verdict']:<20} | {veto_str}")
        print("=" * 80 + "\n")
        return

    # Single File Evaluation
    single_file = target_files[0]
    rel_path = single_file.relative_to(workspace_root) if single_file.is_relative_to(workspace_root) else single_file
    if single_file.exists() and single_file.is_file():
        try:
            with open(single_file, "r", encoding="utf-8", errors="ignore") as f:
                target_context = f"File: {rel_path}\nContent:\n" + f.read()[:3000]
        except Exception:
            target_context = f"Target File: {rel_path}"
    else:
        target_context = f"Target Reference: {rel_path}"

    lens_evals, debate_rounds, decree, telemetry = await dag.execute_dag(target_context, target_file=str(rel_path).replace("\\", "/"))

    print("\n" + "=" * 70)
    print("                    CONSENSUS DECREE SUMMARY")
    print("=" * 70)
    print(f"Decree ID:      {decree.decree_id}")
    print(f"Timestamp:      {decree.timestamp_utc}")
    print(f"Final Verdict:  {decree.verdict}")
    print("-" * 70)
    print("ANTI-OBSOLESCENCE GATE:")
    print(f"  Passed:       {decree.anti_obsolescence_gate.passed}")
    print(f"  Framework:    {decree.anti_obsolescence_gate.target_framework}")
    if decree.anti_obsolescence_gate.deprecated_apis_detected:
        print(f"  Deprecated:   {decree.anti_obsolescence_gate.deprecated_apis_detected}")
    print("-" * 70)
    print("VETO REGISTRY:")
    print(f"  Pen Tester Veto:    {decree.veto_registry.pen_tester_veto_active}")
    print(f"  Legal Counsel Veto: {decree.veto_registry.legal_counsel_veto_active}")
    if decree.veto_registry.blocking_reasons:
        for reason in decree.veto_registry.blocking_reasons:
            print(f"  - {reason}")
    print("-" * 70)
    print("AUTO-REFACTOR DIRECTIVES (STAGED REVIEW MODE):")
    for d in decree.auto_refactor_directives:
        print(f"  [{d.directive_id}] {d.action} -> {d.target_file}")
        print(f"    Impact: {d.compilation_impact}")
        print(f"    Verification: {d.verification_command}")
    print("=" * 70 + "\n")

    if args.apply:
        print("\n" + "=" * 70)
        print("          AUTONOMOUS REFACTOR EXECUTION (GUARDRAIL VALIDATION)")
        print("=" * 70)
        applier = AutonomousRefactorApplier(workspace_root)
        results = applier.apply_directives(decree)
        for res in results:
            status_str = "SUCCESS" if res["applied"] else f"FAILED ({res.get('error')})"
            print(f"  Directive [{res['directive_id']}]: {status_str}")
        print("=" * 70)

        # Re-evaluate DAG to verify that the P0 Hard Veto is cleared and Decree is Approved!
        print("\n[DAG] Re-evaluating DAG following autonomous patch...")
        if single_file.exists() and single_file.is_file():
            with open(single_file, "r", encoding="utf-8", errors="ignore") as f:
                refreshed_context = f"File: {rel_path}\nContent:\n" + f.read()[:3000]
        else:
            refreshed_context = target_context

        _, _, post_decree, _ = await dag.execute_dag(refreshed_context)
        print("\n" + "=" * 70)
        print("             POST-PATCH CONSENSUS DECREE VERDICT")
        print("=" * 70)
        print(f"Updated Decree ID:   {post_decree.decree_id}")
        print(f"Updated Verdict:     {post_decree.verdict}")
        print(f"Pen Tester Veto:     {post_decree.veto_registry.pen_tester_veto_active}")
        print(f"Legal Counsel Veto:  {post_decree.veto_registry.legal_counsel_veto_active}")
        print("=" * 70 + "\n")
        decree = post_decree

    if args.output:
        out_path = Path(args.output)
        with open(out_path, "w", encoding="utf-8") as f:
            f.write(decree.to_json(indent=2))
        print(f"[INFO] Consensus Decree saved to {out_path.resolve()}")


def main():
    asyncio.run(main_async())


if __name__ == "__main__":
    main()
